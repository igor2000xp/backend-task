using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace BlogPlatform.Api.Extensions;

/// <summary>
/// Extension methods for configuring the HTTP request pipeline
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline with all necessary middleware
    /// </summary>
    public static WebApplication UsePipelineConfiguration(this WebApplication app)
    {
        // Development-only middleware
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Blog Platform API")
                    .WithTheme(ScalarTheme.BluePlanet)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        // Security headers
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            await next();
        });

        // Rate Limiting (must come before authentication)
        app.UseRateLimiter();

        app.UseHttpsRedirection();

        // Authentication & Authorization (ORDER MATTERS!)
        app.UseAuthentication();
        app.UseAuthorization();

        // Map Controllers (will be removed after full migration to Minimal APIs)
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Initializes the database and seeds data
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        // Skip database initialization for Testing environment (handled by test factory)
        if (app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<BlogsContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply pending migrations in development
            if (app.Environment.IsDevelopment())
            {
                await context.Database.MigrateAsync();
            }

            // Seed roles and default users
            await SeedData.InitializeAsync(services, userManager, roleManager);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
