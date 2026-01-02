using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Domain.Interfaces;

public interface IBlogRepository
{
    Task<BlogEntity?> GetByIdAsync(int id);
    Task<IEnumerable<BlogEntity>> GetAllAsync();
    Task<BlogEntity> CreateAsync(BlogEntity blog);
    Task UpdateAsync(BlogEntity blog);
    Task DeleteAsync(int id);
}

