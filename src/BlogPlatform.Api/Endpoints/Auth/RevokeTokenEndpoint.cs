using System.Security.Claims;
using BlogPlatform.Api.Abstractions;
using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for revoking all refresh tokens for a user
/// </summary>
public class RevokeTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/revoke-all/{userId?}", async (
            string? userId,
            IAuthService authService,
            ClaimsPrincipal user) =>
        {
            var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Results.Json(new { message = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            // If no userId specified, revoke for current user
            var targetUserId = userId ?? currentUserId;

            // Only allow revoking own tokens, or if admin
            if (targetUserId != currentUserId && !user.IsInRole("Admin"))
            {
                return Results.Forbid();
            }

            var success = await authService.RevokeAllTokensAsync(targetUserId);

            if (!success)
            {
                return Results.NotFound(new { message = "User not found" });
            }

            return Results.Ok(new { message = "All tokens revoked successfully" });
        })
        .WithTags("Auth")
        .WithSummary("Revoke all refresh tokens for a user")
        .WithDescription("Force logout from all devices by revoking all refresh tokens. Users can revoke their own tokens; admins can revoke any user's tokens.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization()
        .RequireRateLimiting("auth");
    }
}
