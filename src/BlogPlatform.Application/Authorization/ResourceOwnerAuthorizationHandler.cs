using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Application.Authorization;

/// <summary>
/// Authorization handler that checks if the current user owns the resource or is an Admin.
/// </summary>
public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, IResourceWithOwner>
{
    private readonly ILogger<ResourceOwnerAuthorizationHandler> _logger;

    public ResourceOwnerAuthorizationHandler(ILogger<ResourceOwnerAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        IResourceWithOwner resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Authorization failed: No user ID in claims");
            return Task.CompletedTask;
        }

        // Check if user is Admin - Admins can access any resource
        if (context.User.IsInRole("Admin"))
        {
            _logger.LogDebug("Admin access granted for resource owned by {OwnerId}", resource.UserId);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user owns the resource
        if (resource.UserId == userId)
        {
            _logger.LogDebug("Owner access granted for user {UserId}", userId);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        _logger.LogWarning(
            "Authorization denied: User {UserId} attempted to access resource owned by {OwnerId}",
            userId, resource.UserId);
        
        return Task.CompletedTask;
    }
}

