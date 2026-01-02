using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Domain.Entities;

public class BlogEntity
{
    public int BlogId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 10)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public ICollection<PostEntity> Articles { get; set; }

    public BlogEntity()
    {
        Articles = new List<PostEntity>();
    }
}

