using BlogPlatform.Application.DTOs;

namespace BlogPlatform.Application.Services.Interfaces;

public interface IBlogService
{
    Task<BlogDto?> GetBlogByIdAsync(int id);
    Task<IEnumerable<BlogDto>> GetAllBlogsAsync();
    Task<BlogDto> CreateBlogAsync(CreateBlogRequest request);
    Task UpdateBlogAsync(int id, UpdateBlogRequest request);
    Task DeleteBlogAsync(int id);
}

