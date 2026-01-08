using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.Services.Interfaces;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;

namespace BlogPlatform.Application.Services;

public class BlogService : IBlogService
{
    private readonly IBlogRepository _blogRepository;

    public BlogService(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<BlogDto?> GetBlogByIdAsync(int id)
    {
        var blog = await _blogRepository.GetByIdAsync(id);
        return blog != null ? MapToDto(blog) : null;
    }

    public async Task<IEnumerable<BlogDto>> GetAllBlogsAsync()
    {
        var blogs = await _blogRepository.GetAllAsync();
        return blogs.Select(MapToDto);
    }

    public async Task<BlogDto> CreateBlogAsync(CreateBlogRequest request)
    {
        var blog = new BlogEntity
        {
            Name = request.Name,
            IsActive = request.IsActive
        };

        var created = await _blogRepository.CreateAsync(blog);
        return MapToDto(created);
    }

    public async Task UpdateBlogAsync(int id, UpdateBlogRequest request)
    {
        var blog = await _blogRepository.GetByIdAsync(id);
        if (blog == null)
        {
            throw new KeyNotFoundException($"Blog with ID {id} not found");
        }

        blog.Name = request.Name;
        blog.IsActive = request.IsActive;

        await _blogRepository.UpdateAsync(blog);
    }

    public async Task DeleteBlogAsync(int id)
    {
        await _blogRepository.DeleteAsync(id);
    }

    private static BlogDto MapToDto(BlogEntity blog)
    {
        return new BlogDto
        {
            Id = blog.BlogId,
            Name = blog.Name,
            IsActive = blog.IsActive,
            ArticleCount = blog.Articles?.Count ?? 0
        };
    }
}

