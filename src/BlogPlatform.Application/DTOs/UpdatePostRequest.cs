using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Application.DTOs;

public class UpdatePostRequest
{
    [Required]
    [StringLength(50, MinimumLength = 10)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;
}

