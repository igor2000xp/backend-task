using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Domain.Interfaces;

public interface IPostRepository
{
    Task<PostEntity?> GetByIdAsync(int id);
    Task<IEnumerable<PostEntity>> GetAllAsync();
    Task<IEnumerable<PostEntity>> GetByBlogIdAsync(int blogId);
    Task<PostEntity> CreateAsync(PostEntity post);
    Task UpdateAsync(PostEntity post);
    Task DeleteAsync(int id);
}

