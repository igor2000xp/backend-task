using BlogPlatform.Application.DTOs.Auth;

namespace BlogPlatform.Application.Services.Interfaces;

/// <summary>
/// Service for handling user authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the provided details
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Authentication result with tokens if successful</returns>
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with tokens if successful</returns>
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Refreshes an access token using a valid refresh token
    /// </summary>
    /// <param name="accessToken">The expired access token</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>Authentication result with new tokens if successful</returns>
    Task<AuthenticationResult> RefreshTokenAsync(string accessToken, string refreshToken);
    
    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="refreshToken">The refresh token to invalidate</param>
    /// <returns>True if logout successful</returns>
    Task<bool> LogoutAsync(string userId, string refreshToken);
    
    /// <summary>
    /// Revokes all refresh tokens for a user (force logout from all devices)
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>True if revocation successful</returns>
    Task<bool> RevokeAllTokensAsync(string userId);
    
    /// <summary>
    /// Gets the current user's information by ID
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>User info if found</returns>
    Task<AuthenticationResult?> GetUserInfoAsync(string userId);
}

