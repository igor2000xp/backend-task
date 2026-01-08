using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly BlogsContext _context;

    public PostRepository(BlogsContext context)
    {
        _context = context;
    }

    public async Task<PostEntity?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.Blog)
            .FirstOrDefaultAsync(p => p.PostId == id);
    }

    public async Task<IEnumerable<PostEntity>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.Blog)
            .ToListAsync();
    }

    public async Task<IEnumerable<PostEntity>> GetByBlogIdAsync(int blogId)
    {
        return await _context.Posts
            .Include(p => p.Blog)
            .Where(p => p.ParentId == blogId)
            .ToListAsync();
    }

    public async Task<PostEntity> CreateAsync(PostEntity post)
    {
        post.Created = DateTime.UtcNow;
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task UpdateAsync(PostEntity post)
    {
        post.Updated = DateTime.UtcNow;
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}

