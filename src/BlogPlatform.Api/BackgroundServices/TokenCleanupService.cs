using BlogPlatform.Application.Services.Interfaces;

namespace BlogPlatform.Api.BackgroundServices;

/// <summary>
/// Background service that periodically cleans up expired compromised tokens from the database
/// </summary>
public class TokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run once daily

    public TokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<TokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoCleanupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Token Cleanup Service is stopping");
    }

    private async Task DoCleanupAsync()
    {
        _logger.LogDebug("Starting token cleanup...");
        
        using var scope = _scopeFactory.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        await tokenService.CleanupExpiredCompromisedTokensAsync();
        
        _logger.LogDebug("Token cleanup completed");
    }
}

