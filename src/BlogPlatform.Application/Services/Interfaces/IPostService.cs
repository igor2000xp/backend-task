using BlogPlatform.Application.DTOs;

namespace BlogPlatform.Application.Services.Interfaces;

public interface IPostService
{
    Task<PostDto?> GetPostByIdAsync(int id);
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<IEnumerable<PostDto>> GetPostsByBlogIdAsync(int blogId);
    Task<PostDto> CreatePostAsync(CreatePostRequest request);
    Task UpdatePostAsync(int id, UpdatePostRequest request);
    Task DeletePostAsync(int id);
}

