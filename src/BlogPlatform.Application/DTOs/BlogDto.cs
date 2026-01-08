namespace BlogPlatform.Application.DTOs;

public class BlogDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ArticleCount { get; set; }
}

