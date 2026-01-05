using System.Security.Claims;
using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Services.Interfaces;

/// <summary>
/// Service for JWT token generation, validation, and management
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user
    /// </summary>
    /// <param name="user">The user to generate the token for</param>
    /// <param name="roles">The user's roles to include in claims</param>
    /// <returns>JWT access token string</returns>
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);
    
    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates an access token and returns whether it's valid
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateAccessTokenAsync(string token);
    
    /// <summary>
    /// Extracts the ClaimsPrincipal from an expired token (for refresh flow)
    /// </summary>
    /// <param name="token">The expired access token</param>
    /// <returns>ClaimsPrincipal if token is valid (except expiration), null otherwise</returns>
    Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token);
    
    /// <summary>
    /// Checks if a refresh token has been compromised/blacklisted
    /// </summary>
    /// <param name="refreshToken">The refresh token to check</param>
    /// <returns>True if compromised, false if valid</returns>
    Task<bool> IsRefreshTokenCompromisedAsync(string refreshToken);
    
    /// <summary>
    /// Adds a refresh token to the compromised/blacklist table
    /// </summary>
    /// <param name="refreshToken">The refresh token to blacklist</param>
    /// <param name="reason">Reason for blacklisting</param>
    Task CompromiseRefreshTokenAsync(string refreshToken, string reason);
    
    /// <summary>
    /// Removes expired entries from the compromised tokens table
    /// </summary>
    Task CleanupExpiredCompromisedTokensAsync();
    
    /// <summary>
    /// Gets the access token expiration time based on settings
    /// </summary>
    DateTime GetAccessTokenExpiration();
    
    /// <summary>
    /// Gets the refresh token expiration time based on settings
    /// </summary>
    DateTime GetRefreshTokenExpiration();
}

