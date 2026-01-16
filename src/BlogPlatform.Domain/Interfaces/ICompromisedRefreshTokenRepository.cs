using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Domain.Interfaces;

public interface ICompromisedRefreshTokenRepository
{
    Task<bool> IsCompromisedAsync(string tokenHash);
    Task AddAsync(CompromisedRefreshToken compromisedToken);
    Task RemoveExpiredAsync(DateTime cutoffDate);
}
