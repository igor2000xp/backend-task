using BlogPlatform.Application.DTOs;

namespace BlogPlatform.Application.Services.Interfaces;

public interface IPostService
{
    Task<PostDto?> GetPostByIdAsync(int id);
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<IEnumerable<PostDto>> GetPostsByBlogIdAsync(int blogId);
    
    /// <summary>
    /// Gets all posts created by a specific user
    /// </summary>
    Task<IEnumerable<PostDto>> GetUserPostsAsync(string userId);
    
    /// <summary>
    /// Creates a new post owned by the specified user
    /// </summary>
    Task<PostDto> CreatePostAsync(CreatePostRequest request, string userId);
    
    /// <summary>
    /// Updates a post. Throws UnauthorizedAccessException if user doesn't own the post (unless admin).
    /// </summary>
    Task UpdatePostAsync(int id, UpdatePostRequest request, string userId, bool isAdmin);
    
    /// <summary>
    /// Deletes a post. Throws UnauthorizedAccessException if user doesn't own the post (unless admin).
    /// </summary>
    Task DeletePostAsync(int id, string userId, bool isAdmin);
}
