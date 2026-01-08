using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services;
using BlogPlatform.Application.Services.Interfaces;
using BlogPlatform.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace BlogPlatform.Application.Tests.Services;

[TestClass]
public class AuthServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;
    private Mock<SignInManager<ApplicationUser>> _signInManagerMock = null!;
    private Mock<ITokenService> _tokenServiceMock = null!;
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private AuthService _authService = null!;

    [TestInitialize]
    public void Setup()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            userClaimsPrincipalFactoryMock.Object,
            null!, null!, null!, null!);

        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "Password@123",
            ConfirmPassword = "Password@123",
            FullName = "New User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
            .ReturnsAsync("test-access-token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        _tokenServiceMock.Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("test-access-token", result.AccessToken);
        Assert.AreEqual("test-refresh-token", result.RefreshToken);
        Assert.AreEqual(request.Email, result.Email);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [TestMethod]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@test.com",
            Password = "Password@123",
            ConfirmPassword = "Password@123",
            FullName = "Existing User"
        };

        var existingUser = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("already exists")));
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_WhenCreateFails_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "weak",
            ConfirmPassword = "weak",
            FullName = "New User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password must be at least 8 characters." }));

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Password must be at least 8 characters")));
    }

    [TestMethod]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "Password@123"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email,
            FullName = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user, It.IsAny<IList<string>>()))
            .ReturnsAsync("test-access-token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        _tokenServiceMock.Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("test-access-token", result.AccessToken);
        Assert.AreEqual("test-refresh-token", result.RefreshToken);
        Assert.AreEqual(user.Id, result.UserId);
        _signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(user, request.Password, true), Times.Once);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "WrongPassword"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid")));
    }

    [TestMethod]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "WrongPassword"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email,
            FullName = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid")));
    }

    [TestMethod]
    public async Task LoginAsync_WithLockedOutUser_BeforePasswordCheck_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "locked@test.com",
            Password = "Password@123"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetLockoutEndDateAsync(user))
            .ReturnsAsync(DateTimeOffset.UtcNow.AddMinutes(10));

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("locked")));
        // Password check should not be called if already locked out
        _signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [TestMethod]
    public async Task LoginAsync_WhenPasswordCheckCausesLockout_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "WrongPassword"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("locked")));
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithValidTokens_ReturnsNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var accessToken = "valid-access-token";
        var refreshToken = "valid-refresh-token";

        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@test.com",
            UserName = "user@test.com",
            FullName = "Test User"
        };

        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

        _tokenServiceMock.Setup(x => x.IsRefreshTokenCompromisedAsync(refreshToken))
            .ReturnsAsync(false);

        _tokenServiceMock.Setup(x => x.GetPrincipalFromExpiredTokenAsync(accessToken))
            .ReturnsAsync(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _tokenServiceMock.Setup(x => x.CompromiseRefreshTokenAsync(refreshToken, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _tokenServiceMock.Setup(x => x.CleanupExpiredCompromisedTokensAsync())
            .Returns(Task.CompletedTask);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user, It.IsAny<IList<string>>()))
            .ReturnsAsync("new-access-token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _tokenServiceMock.Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        var result = await _authService.RefreshTokenAsync(accessToken, refreshToken);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("new-access-token", result.AccessToken);
        Assert.AreEqual("new-refresh-token", result.RefreshToken);
        _tokenServiceMock.Verify(x => x.CompromiseRefreshTokenAsync(refreshToken, "token_refresh"), Times.Once);
        _tokenServiceMock.Verify(x => x.CleanupExpiredCompromisedTokensAsync(), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithCompromisedToken_ReturnsFailure()
    {
        // Arrange
        var accessToken = "valid-access-token";
        var refreshToken = "compromised-refresh-token";

        _tokenServiceMock.Setup(x => x.IsRefreshTokenCompromisedAsync(refreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RefreshTokenAsync(accessToken, refreshToken);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid refresh token")));
        // Should not attempt to get principal if refresh token is compromised
        _tokenServiceMock.Verify(x => x.GetPrincipalFromExpiredTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithInvalidAccessToken_ReturnsFailure()
    {
        // Arrange
        var accessToken = "invalid-access-token";
        var refreshToken = "valid-refresh-token";

        _tokenServiceMock.Setup(x => x.IsRefreshTokenCompromisedAsync(refreshToken))
            .ReturnsAsync(false);

        _tokenServiceMock.Setup(x => x.GetPrincipalFromExpiredTokenAsync(accessToken))
            .ReturnsAsync((ClaimsPrincipal?)null);

        // Act
        var result = await _authService.RefreshTokenAsync(accessToken, refreshToken);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid access token")));
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithLockedOutUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var accessToken = "valid-access-token";
        var refreshToken = "valid-refresh-token";

        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

        _tokenServiceMock.Setup(x => x.IsRefreshTokenCompromisedAsync(refreshToken))
            .ReturnsAsync(false);

        _tokenServiceMock.Setup(x => x.GetPrincipalFromExpiredTokenAsync(accessToken))
            .ReturnsAsync(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RefreshTokenAsync(accessToken, refreshToken);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("locked")));
    }

    [TestMethod]
    public async Task LogoutAsync_BlacklistsRefreshToken()
    {
        // Arrange
        var userId = "test-user-id";
        var refreshToken = "test-refresh-token";

        _tokenServiceMock.Setup(x => x.CompromiseRefreshTokenAsync(refreshToken, "logout"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync(userId, refreshToken);

        // Assert
        Assert.IsTrue(result);
        _tokenServiceMock.Verify(x => x.CompromiseRefreshTokenAsync(refreshToken, "logout"), Times.Once);
    }

    [TestMethod]
    public async Task RevokeAllTokensAsync_WithValidUser_UpdatesSecurityStamp()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RevokeAllTokensAsync(userId);

        // Assert
        Assert.IsTrue(result);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task RevokeAllTokensAsync_WithInvalidUser_ReturnsFalse()
    {
        // Arrange
        var userId = "non-existent-user";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.RevokeAllTokensAsync(userId);

        // Assert
        Assert.IsFalse(result);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [TestMethod]
    public async Task GetUserInfoAsync_WithValidUser_ReturnsUserInfo()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@test.com",
            UserName = "user@test.com",
            FullName = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User", "Admin" });

        // Act
        var result = await _authService.GetUserInfoAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(userId, result.UserId);
        Assert.AreEqual("user@test.com", result.Email);
        Assert.AreEqual("Test User", result.FullName);
        Assert.IsNotNull(result.Roles);
        Assert.AreEqual(2, result.Roles.Count);
    }

    [TestMethod]
    public async Task GetUserInfoAsync_WithInvalidUser_ReturnsNull()
    {
        // Arrange
        var userId = "non-existent-user";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.GetUserInfoAsync(userId);

        // Assert
        Assert.IsNull(result);
    }
}
