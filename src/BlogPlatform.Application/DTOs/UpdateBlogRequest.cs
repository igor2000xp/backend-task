using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Application.DTOs;

public class UpdateBlogRequest
{
    [Required]
    [StringLength(50, MinimumLength = 10)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
}

