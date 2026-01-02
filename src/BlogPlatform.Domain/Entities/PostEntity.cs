using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Domain.Entities;

public class PostEntity
{
    public int PostId { get; set; }
    
    public int ParentId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 10)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime Created { get; set; }
    
    public DateTime? Updated { get; set; }

    // Navigation property required for explicit relationship mapping
    public BlogEntity Blog { get; set; } = null!;
}

