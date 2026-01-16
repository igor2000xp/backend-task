using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.Services.Interfaces;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;

namespace BlogPlatform.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IBlogRepository _blogRepository;

    public PostService(IPostRepository postRepository, IBlogRepository blogRepository)
    {
        _postRepository = postRepository;
        _blogRepository = blogRepository;
    }

    public async Task<PostDto?> GetPostByIdAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        return post != null ? MapToDto(post) : null;
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return posts.Select(MapToDto);
    }

    public async Task<IEnumerable<PostDto>> GetPostsByBlogIdAsync(int blogId)
    {
        var posts = await _postRepository.GetByBlogIdAsync(blogId);
        return posts.Select(MapToDto);
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(string userId)
    {
        var posts = await _postRepository.GetAllAsync();
        return posts.Where(p => p.UserId == userId).Select(MapToDto);
    }

    public async Task<PostDto> CreatePostAsync(CreatePostRequest request, string userId)
    {
        // Verify blog exists
        var blog = await _blogRepository.GetByIdAsync(request.BlogId);
        if (blog == null)
        {
            throw new KeyNotFoundException($"Blog with ID {request.BlogId} not found");
        }

        var post = new PostEntity
        {
            Name = request.Name,
            Content = request.Content,
            ParentId = request.BlogId,
            UserId = userId,
            Created = DateTime.UtcNow
        };

        var created = await _postRepository.CreateAsync(post);
        return MapToDto(created);
    }

    public async Task UpdatePostAsync(int id, UpdatePostRequest request, string userId, bool isAdmin)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {id} not found");
        }

        // Check ownership unless admin
        if (!isAdmin && post.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this post.");
        }

        post.Name = request.Name;
        post.Content = request.Content;
        post.Updated = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);
    }

    public async Task DeletePostAsync(int id, string userId, bool isAdmin)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {id} not found");
        }

        // Check ownership unless admin
        if (!isAdmin && post.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this post.");
        }

        await _postRepository.DeleteAsync(id);
    }

    private static PostDto MapToDto(PostEntity post)
    {
        return new PostDto
        {
            Id = post.PostId,
            Name = post.Name,
            Content = post.Content,
            Created = post.Created,
            Updated = post.Updated,
            BlogId = post.ParentId,
            BlogName = post.Blog?.Name ?? string.Empty,
            UserId = post.UserId,
            AuthorName = post.User?.FullName ?? string.Empty
        };
    }
}
