using System.ComponentModel.DataAnnotations;
using BlogPlatform.Api.Abstractions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for user login
/// </summary>
public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
        {
            // Validate DataAnnotations manually (Minimal APIs don't do this automatically)
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation error").ToList();
                return Results.BadRequest(AuthenticationResult.Failed(errors));
            }

            var result = await authService.LoginAsync(request);

            if (!result.Success)
            {
                return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithSummary("Login with email and password")
        .WithDescription("Authenticates a user and returns access and refresh tokens")
        .Produces<AuthenticationResult>(StatusCodes.Status200OK)
        .Produces<AuthenticationResult>(StatusCodes.Status401Unauthorized)
        .RequireRateLimiting("auth");
    }
}
