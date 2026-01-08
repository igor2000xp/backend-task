using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Domain.Entities;

/// <summary>
/// Stores blacklisted/compromised refresh tokens that should be rejected.
/// Tokens are stored as hashes and removed after their expiration date passes.
/// </summary>
public class CompromisedRefreshToken
{
    public int Id { get; set; }
    
    /// <summary>
    /// SHA256 hash of the refresh token
    /// </summary>
    [Required]
    [StringLength(512)]
    public string TokenHash { get; set; } = string.Empty;
    
    /// <summary>
    /// When this token was marked as compromised/revoked
    /// </summary>
    public DateTime CompromisedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the original token would have expired (used for cleanup)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Reason for blacklisting (e.g., "logout", "password_change", "admin_revoke")
    /// </summary>
    [StringLength(100)]
    public string? Reason { get; set; }
}

