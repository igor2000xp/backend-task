using System.ComponentModel.DataAnnotations;
using BlogPlatform.Api.Abstractions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for user registration
/// </summary>
public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, IAuthService authService) =>
        {
            // Validate DataAnnotations manually (Minimal APIs don't do this automatically)
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation error").ToList();
                return Results.BadRequest(AuthenticationResult.Failed(errors));
            }

            var result = await authService.RegisterAsync(request);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account and returns authentication tokens")
        .Produces<AuthenticationResult>(StatusCodes.Status200OK)
        .Produces<AuthenticationResult>(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("auth");
    }
}
