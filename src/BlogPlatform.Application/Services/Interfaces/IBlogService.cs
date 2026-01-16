using BlogPlatform.Application.DTOs;

namespace BlogPlatform.Application.Services.Interfaces;

public interface IBlogService
{
    Task<BlogDto?> GetBlogByIdAsync(int id);
    Task<IEnumerable<BlogDto>> GetAllBlogsAsync();
    
    /// <summary>
    /// Gets all blogs owned by a specific user
    /// </summary>
    Task<IEnumerable<BlogDto>> GetUserBlogsAsync(string userId);
    
    /// <summary>
    /// Creates a new blog owned by the specified user
    /// </summary>
    Task<BlogDto> CreateBlogAsync(CreateBlogRequest request, string userId);
    
    /// <summary>
    /// Updates a blog. Throws UnauthorizedAccessException if user doesn't own the blog (unless admin).
    /// </summary>
    Task UpdateBlogAsync(int id, UpdateBlogRequest request, string userId, bool isAdmin);
    
    /// <summary>
    /// Deletes a blog. Throws UnauthorizedAccessException if user doesn't own the blog (unless admin).
    /// </summary>
    Task DeleteBlogAsync(int id, string userId, bool isAdmin);
}
