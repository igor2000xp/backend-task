using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlogPlatform.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties for owned resources
    public ICollection<BlogEntity> Blogs { get; set; } = new List<BlogEntity>();
    public ICollection<PostEntity> Posts { get; set; } = new List<PostEntity>();
}

