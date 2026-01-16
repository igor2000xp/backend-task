using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogPlatform.Application.Configuration;
using BlogPlatform.Application.Services.Interfaces;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlogPlatform.Application.Services;

/// <summary>
/// Implementation of token generation, validation, and blacklist management
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ICompromisedRefreshTokenRepository _repository;
    private readonly ILogger<TokenService> _logger;
    private readonly SymmetricSecurityKey _signingKey;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        ICompromisedRefreshTokenRepository repository,
        ILogger<TokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _repository = repository;
        _logger = logger;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    }

    /// <inheritdoc />
    public Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expires = GetAccessTokenExpiration();

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("Generated access token for user {UserId}, expires at {Expiration}", 
            user.Id, expires);
        
        return Task.FromResult(tokenString);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public Task<bool> ValidateAccessTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var validationParameters = GetTokenValidationParameters();
            tokenHandler.ValidateToken(token, validationParameters, out _);
            return Task.FromResult(true);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token validation failed: Token expired");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Token validation failed: {Message}", ex.Message);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            // Don't validate lifetime - we expect the token to be expired
            var validationParameters = GetTokenValidationParameters(validateLifetime: false);
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            
            // Verify this is actually a JWT token
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm detected");
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
            
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get principal from token: {Message}", ex.Message);
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsRefreshTokenCompromisedAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        
        var isCompromised = await _repository.IsCompromisedAsync(tokenHash);
        
        if (isCompromised)
        {
            _logger.LogWarning("Attempted use of compromised refresh token");
        }
        
        return isCompromised;
    }

    /// <inheritdoc />
    public async Task CompromiseRefreshTokenAsync(string refreshToken, string reason)
    {
        var tokenHash = HashToken(refreshToken);
        
        // Repository handles duplicate check
        var compromisedToken = new CompromisedRefreshToken
        {
            TokenHash = tokenHash,
            CompromisedAt = DateTime.UtcNow,
            ExpiresAt = GetRefreshTokenExpiration(),
            Reason = reason
        };
        
        await _repository.AddAsync(compromisedToken);
        
        _logger.LogInformation("Refresh token compromised: {Reason}", reason);
    }

    /// <inheritdoc />
    public async Task CleanupExpiredCompromisedTokensAsync()
    {
        var now = DateTime.UtcNow;
        
        await _repository.RemoveExpiredAsync(now);
        
        _logger.LogInformation("Cleaned up expired compromised tokens");
    }

    /// <inheritdoc />
    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
    }

    /// <inheritdoc />
    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
    }

    private TokenValidationParameters GetTokenValidationParameters(bool validateLifetime = true)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero // No tolerance for expiration
        };
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
