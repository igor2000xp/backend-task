namespace BlogPlatform.Application.DTOs;

public class PostDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public int BlogId { get; set; }
    public string BlogName { get; set; } = string.Empty;
}

