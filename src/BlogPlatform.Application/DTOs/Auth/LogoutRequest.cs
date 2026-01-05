using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user logout
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// The refresh token to invalidate
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

