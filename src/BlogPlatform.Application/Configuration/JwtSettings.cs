namespace BlogPlatform.Application.Configuration;

/// <summary>
/// Configuration settings for JWT token generation and validation
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    /// <summary>
    /// Secret key used to sign JWT tokens (minimum 32 characters for security)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Token issuer (typically the API domain)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Token audience (typically the client application)
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Access token expiration time in minutes (default: 15)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    
    /// <summary>
    /// Refresh token expiration time in days (default: 7)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

