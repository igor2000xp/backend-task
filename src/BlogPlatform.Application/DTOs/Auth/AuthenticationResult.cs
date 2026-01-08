namespace BlogPlatform.Application.DTOs.Auth;

/// <summary>
/// Result DTO for authentication operations (login, register, refresh)
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Indicates whether the authentication operation succeeded
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// JWT access token (valid for 15 minutes)
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Refresh token for obtaining new access tokens (valid for 7 days)
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// The authenticated user's ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// The authenticated user's email
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// The authenticated user's full name
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// List of roles assigned to the user
    /// </summary>
    public IList<string>? Roles { get; set; }
    
    /// <summary>
    /// Access token expiration time (UTC)
    /// </summary>
    public DateTime? AccessTokenExpiration { get; set; }
    
    /// <summary>
    /// List of error messages if authentication failed
    /// </summary>
    public IList<string>? Errors { get; set; }
    
    /// <summary>
    /// Creates a successful authentication result
    /// </summary>
    public static AuthenticationResult Succeeded(
        string accessToken, 
        string refreshToken, 
        string userId, 
        string email,
        string fullName,
        IList<string> roles,
        DateTime accessTokenExpiration)
    {
        return new AuthenticationResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = userId,
            Email = email,
            FullName = fullName,
            Roles = roles,
            AccessTokenExpiration = accessTokenExpiration
        };
    }
    
    /// <summary>
    /// Creates a failed authentication result
    /// </summary>
    public static AuthenticationResult Failed(params string[] errors)
    {
        return new AuthenticationResult
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
    
    /// <summary>
    /// Creates a failed authentication result from multiple errors
    /// </summary>
    public static AuthenticationResult Failed(IEnumerable<string> errors)
    {
        return new AuthenticationResult
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
}

