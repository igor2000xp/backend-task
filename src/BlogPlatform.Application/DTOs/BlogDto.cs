using BlogPlatform.Application.Authorization;

namespace BlogPlatform.Application.DTOs;

public class BlogDto : IResourceWithOwner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ArticleCount { get; set; }
    
    /// <summary>
    /// The ID of the user who owns this blog
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the user who owns this blog
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
}
