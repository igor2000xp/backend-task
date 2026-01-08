using BlogPlatform.Application.Authorization;

namespace BlogPlatform.Application.DTOs;

public class PostDto : IResourceWithOwner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public int BlogId { get; set; }
    public string BlogName { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the user who created this post
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the user who created this post
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
}
