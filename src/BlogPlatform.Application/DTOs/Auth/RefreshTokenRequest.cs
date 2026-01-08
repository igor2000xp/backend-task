using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Application.DTOs.Auth;

/// <summary>
/// Request DTO for refreshing access tokens
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The expired or about-to-expire access token
    /// </summary>
    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// The refresh token issued during login
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

