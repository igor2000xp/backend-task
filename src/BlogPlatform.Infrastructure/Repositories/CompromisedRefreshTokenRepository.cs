using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Repositories;

public class CompromisedRefreshTokenRepository : ICompromisedRefreshTokenRepository
{
    private readonly BlogsContext _context;

    public CompromisedRefreshTokenRepository(BlogsContext context)
    {
        _context = context;
    }

    public async Task<bool> IsCompromisedAsync(string tokenHash)
    {
        return await _context.CompromisedRefreshTokens
            .AnyAsync(t => t.TokenHash == tokenHash);
    }

    public async Task AddAsync(CompromisedRefreshToken compromisedToken)
    {
        // Check if already exists to prevent duplicates
        var exists = await _context.CompromisedRefreshTokens
            .AnyAsync(t => t.TokenHash == compromisedToken.TokenHash);
        
        if (!exists)
        {
            _context.CompromisedRefreshTokens.Add(compromisedToken);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveExpiredAsync(DateTime cutoffDate)
    {
        var expiredTokens = await _context.CompromisedRefreshTokens
            .Where(t => t.ExpiresAt < cutoffDate)
            .ToListAsync();
        
        if (expiredTokens.Count > 0)
        {
            _context.CompromisedRefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}
