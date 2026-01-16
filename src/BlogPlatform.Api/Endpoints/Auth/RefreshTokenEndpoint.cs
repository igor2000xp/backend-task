using System.ComponentModel.DataAnnotations;
using BlogPlatform.Api.Abstractions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for refreshing access tokens
/// </summary>
public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            // Validate DataAnnotations manually (Minimal APIs don't do this automatically)
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation error").ToList();
                return Results.BadRequest(AuthenticationResult.Failed(errors));
            }

            var result = await authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

            if (!result.Success)
            {
                return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithSummary("Refresh an access token")
        .WithDescription("Uses a valid refresh token to obtain new access and refresh tokens")
        .Produces<AuthenticationResult>(StatusCodes.Status200OK)
        .Produces<AuthenticationResult>(StatusCodes.Status401Unauthorized)
        .RequireRateLimiting("auth");
    }
}
