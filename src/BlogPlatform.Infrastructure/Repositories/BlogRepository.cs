using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Repositories;

public class BlogRepository : IBlogRepository
{
    private readonly BlogsContext _context;

    public BlogRepository(BlogsContext context)
    {
        _context = context;
    }

    public async Task<BlogEntity?> GetByIdAsync(int id)
    {
        return await _context.Blogs
            .Include(b => b.Articles)
            .FirstOrDefaultAsync(b => b.BlogId == id);
    }

    public async Task<IEnumerable<BlogEntity>> GetAllAsync()
    {
        return await _context.Blogs
            .Include(b => b.Articles)
            .ToListAsync();
    }

    public async Task<BlogEntity> CreateAsync(BlogEntity blog)
    {
        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();
        return blog;
    }

    public async Task UpdateAsync(BlogEntity blog)
    {
        _context.Blogs.Update(blog);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog != null)
        {
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
        }
    }
}

