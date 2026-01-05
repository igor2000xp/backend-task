using System.Security.Claims;
using BlogPlatform.Application.Configuration;
using BlogPlatform.Application.Services;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Moq;

namespace BlogPlatform.Application.Tests.Services;

[TestClass]
public class TokenServiceTests
{
    private BlogsContext _context = null!;
    private TokenService _tokenService = null!;
    private JwtSettings _jwtSettings = null!;
    private Mock<ILogger<TokenService>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BlogsContext(options);

        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly32Chars",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _loggerMock = new Mock<ILogger<TokenService>>();
        var jwtOptions = Options.Create(_jwtSettings);
        _tokenService = new TokenService(jwtOptions, _context, _loggerMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task GenerateAccessTokenAsync_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com",
            FullName = "Test User"
        };
        var roles = new List<string> { "User" };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(user, roles);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrEmpty(token));
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        Assert.AreEqual(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.IsTrue(jwtToken.Audiences.Contains(_jwtSettings.Audience));
    }

    [TestMethod]
    public async Task GenerateAccessTokenAsync_IncludesUserClaims()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = email,
            UserName = email,
            FullName = "Test User"
        };
        var roles = new List<string> { "User", "Admin" };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        // ClaimTypes.Email gets serialized as full URI in JWT, check for both
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => 
            c.Type == JwtRegisteredClaimNames.Email || 
            c.Type == ClaimTypes.Email || 
            c.Type == "email");
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").ToList();
        
        Assert.IsNotNull(subClaim);
        Assert.AreEqual(userId, subClaim.Value);
        Assert.IsNotNull(emailClaim);
        Assert.AreEqual(email, emailClaim.Value);
        Assert.AreEqual(2, roleClaims.Count);
    }

    [TestMethod]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.IsNotNull(refreshToken);
        Assert.IsFalse(string.IsNullOrEmpty(refreshToken));
    }

    [TestMethod]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();
        var token3 = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.AreNotEqual(token1, token2);
        Assert.AreNotEqual(token2, token3);
        Assert.AreNotEqual(token1, token3);
    }

    [TestMethod]
    public async Task ValidateAccessTokenAsync_ReturnsTrueForValidToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com",
            FullName = "Test User"
        };
        var token = await _tokenService.GenerateAccessTokenAsync(user, new List<string> { "User" });

        // Act
        var isValid = await _tokenService.ValidateAccessTokenAsync(token);

        // Assert
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public async Task ValidateAccessTokenAsync_ReturnsFalseForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = await _tokenService.ValidateAccessTokenAsync(invalidToken);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public async Task GetPrincipalFromExpiredTokenAsync_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            FullName = "Test User"
        };
        var token = await _tokenService.GenerateAccessTokenAsync(user, new List<string> { "User" });

        // Act
        var principal = await _tokenService.GetPrincipalFromExpiredTokenAsync(token);

        // Assert
        Assert.IsNotNull(principal);
        var nameIdentifier = principal.FindFirst(ClaimTypes.NameIdentifier);
        Assert.IsNotNull(nameIdentifier);
        Assert.AreEqual(userId, nameIdentifier.Value);
    }

    [TestMethod]
    public async Task IsRefreshTokenCompromisedAsync_ReturnsFalseForNewToken()
    {
        // Arrange
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Act
        var isCompromised = await _tokenService.IsRefreshTokenCompromisedAsync(refreshToken);

        // Assert
        Assert.IsFalse(isCompromised);
    }

    [TestMethod]
    public async Task CompromiseRefreshTokenAsync_AddsTokenToBlacklist()
    {
        // Arrange
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Act
        await _tokenService.CompromiseRefreshTokenAsync(refreshToken, "Test reason");
        var isCompromised = await _tokenService.IsRefreshTokenCompromisedAsync(refreshToken);

        // Assert
        Assert.IsTrue(isCompromised);
        Assert.AreEqual(1, await _context.CompromisedRefreshTokens.CountAsync());
    }

    [TestMethod]
    public async Task CleanupExpiredCompromisedTokensAsync_RemovesExpiredTokens()
    {
        // Arrange
        var expiredToken = new CompromisedRefreshToken
        {
            TokenHash = "expired-hash",
            CompromisedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3),
            Reason = "Expired"
        };
        var validToken = new CompromisedRefreshToken
        {
            TokenHash = "valid-hash",
            CompromisedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Reason = "Still valid"
        };
        _context.CompromisedRefreshTokens.AddRange(expiredToken, validToken);
        await _context.SaveChangesAsync();

        // Act
        await _tokenService.CleanupExpiredCompromisedTokensAsync();

        // Assert
        var remainingTokens = await _context.CompromisedRefreshTokens.ToListAsync();
        Assert.AreEqual(1, remainingTokens.Count);
        Assert.AreEqual("valid-hash", remainingTokens[0].TokenHash);
    }
}

