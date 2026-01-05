using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Domain.Entities;

public class BlogEntity
{
    public int BlogId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 10)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    
    /// <summary>
    /// The ID of the user who owns this blog
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to the owner
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    public ICollection<PostEntity> Articles { get; set; }

    public BlogEntity()
    {
        Articles = new List<PostEntity>();
    }
}

