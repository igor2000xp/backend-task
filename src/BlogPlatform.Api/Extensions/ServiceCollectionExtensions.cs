using System.Text;
using System.Threading.RateLimiting;
using BlogPlatform.Api.BackgroundServices;
using BlogPlatform.Api.OpenApi;
using BlogPlatform.Application.Authorization;
using BlogPlatform.Application.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace BlogPlatform.Api.Extensions;

/// <summary>
/// Extension methods for configuring Web API-specific services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Web UI/API specific services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddWebUIServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Configure JWT settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured");
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Background Services
        services.AddHostedService<TokenCleanupService>();

        // JWT Authentication Configuration
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero // No tolerance for token expiration
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    // Log authentication challenges for debugging
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Authentication challenge: {Error}", context.Error);
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization Configuration
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
            options.AddPolicy("ResourceOwner", policy =>
                policy.Requirements.Add(new ResourceOwnerRequirement()));
        });

        // Rate Limiting Configuration
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global fixed window limiter - 100 requests per minute per IP
            options.AddPolicy("fixed", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5
                    }));

            // Stricter limiter for authentication endpoints - 10 requests per minute per IP
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));

            // Sliding window for API endpoints - 50 requests per 30 seconds
            options.AddPolicy("api", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromSeconds(30),
                        SegmentsPerWindow = 3,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Too many requests. Please try again later.",
                        retryAfterSeconds = retryAfter.TotalSeconds
                    }, cancellationToken: token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Too many requests. Please try again later."
                    }, cancellationToken: token);
                }
            };
        });

        // Controllers and API (will be removed after full migration to Minimal APIs)
        services.AddControllers();

        // OpenAPI Configuration with JWT Support
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Blog Platform API";
                document.Info.Version = "v1";
                document.Info.Description = "A secure blog platform API with JWT authentication and role-based authorization";
                return Task.CompletedTask;
            });

            // Add Bearer token authentication using the built-in transformer
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return services;
    }
}
