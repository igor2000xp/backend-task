using System.Security.Claims;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Authentication result with tokens if successful</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with access and refresh tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Refresh an access token using a valid refresh token
    /// </summary>
    /// <param name="request">The expired access token and refresh token</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Logout the current user (invalidates the refresh token)
    /// </summary>
    /// <param name="request">The refresh token to invalidate</param>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        await _authService.LogoutAsync(userId, request.RefreshToken);
        
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Revoke all refresh tokens for a user (force logout from all devices)
    /// </summary>
    /// <param name="userId">The user ID to revoke tokens for (admin only, or self)</param>
    /// <returns>Success status</returns>
    [HttpPost("revoke-all/{userId?}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> RevokeAllTokens(string? userId = null)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // If no userId specified, revoke for current user
        var targetUserId = userId ?? currentUserId;

        // Only allow revoking own tokens, or if admin
        if (targetUserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _authService.RevokeAllTokensAsync(targetUserId);
        
        if (!success)
        {
            return NotFound(new { message = "User not found" });
        }
        
        return Ok(new { message = "All tokens revoked successfully" });
    }

    /// <summary>
    /// Get current user's information
    /// </summary>
    /// <returns>Current user's profile</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _authService.GetUserInfoAsync(userId);
        
        if (result == null)
        {
            return NotFound(new { message = "User not found" });
        }
        
        return Ok(result);
    }
}

