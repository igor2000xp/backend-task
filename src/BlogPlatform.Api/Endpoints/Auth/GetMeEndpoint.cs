using System.Security.Claims;
using BlogPlatform.Api.Abstractions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for getting current user information
/// </summary>
public class GetMeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", async (
            IAuthService authService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Json(new { message = "User not authenticated" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await authService.GetUserInfoAsync(userId);

            if (result == null)
            {
                return Results.NotFound(new { message = "User not found" });
            }

            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithSummary("Get current user's information")
        .WithDescription("Returns the profile information for the authenticated user")
        .Produces<AuthenticationResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .RequireRateLimiting("auth");
    }
}
