using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BlogPlatform.Api.Abstractions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for user logout
/// </summary>
public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/logout", async (
            LogoutRequest request,
            IAuthService authService,
            ClaimsPrincipal user) =>
        {
            // Validate DataAnnotations manually (Minimal APIs don't do this automatically)
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation error").ToList();
                return Results.BadRequest(new { errors });
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { message = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            await authService.LogoutAsync(userId, request.RefreshToken);

            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithTags("Auth")
        .WithSummary("Logout the current user")
        .WithDescription("Invalidates the refresh token to logout the user")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .RequireRateLimiting("auth");
    }
}
