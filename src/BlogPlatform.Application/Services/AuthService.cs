using System.Security.Claims;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Services.Interfaces;
using BlogPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Application.Services;

/// <summary>
/// Implementation of authentication operations using ASP.NET Core Identity
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting to register user: {Email}", request.Email);

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            return AuthenticationResult.Failed("A user with this email already exists.");
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true // Auto-confirm for now; set to false when email service is implemented
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, string.Join(", ", errors));
            return AuthenticationResult.Failed(errors);
        }

        // Assign default "User" role
        await _userManager.AddToRoleAsync(user, "User");
        
        _logger.LogInformation("User {Email} registered successfully", request.Email);

        // Generate tokens for immediate login
        return await GenerateAuthenticationResultAsync(user);
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User {Email} not found", request.Email);
            return AuthenticationResult.Failed("Invalid email or password.");
        }

        // Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            _logger.LogWarning("Login failed: User {Email} is locked out until {LockoutEnd}", request.Email, lockoutEnd);
            return AuthenticationResult.Failed($"Account is locked. Please try again after {lockoutEnd?.UtcDateTime}.");
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} has been locked out due to failed attempts", request.Email);
                return AuthenticationResult.Failed("Account has been locked due to multiple failed login attempts. Please try again later.");
            }
            
            _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
            return AuthenticationResult.Failed("Invalid email or password.");
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        
        return await GenerateAuthenticationResultAsync(user);
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        _logger.LogDebug("Attempting to refresh token");

        // Check if refresh token is compromised
        if (await _tokenService.IsRefreshTokenCompromisedAsync(refreshToken))
        {
            _logger.LogWarning("Refresh token is compromised/blacklisted");
            return AuthenticationResult.Failed("Invalid refresh token. Please login again.");
        }

        // Get principal from expired access token
        var principal = await _tokenService.GetPrincipalFromExpiredTokenAsync(accessToken);
        if (principal == null)
        {
            _logger.LogWarning("Failed to extract principal from access token");
            return AuthenticationResult.Failed("Invalid access token.");
        }

        // Get user ID from claims
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in access token claims");
            return AuthenticationResult.Failed("Invalid access token.");
        }

        // Find user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for refresh token: {UserId}", userId);
            return AuthenticationResult.Failed("User not found.");
        }

        // Check if user is still active
        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Refresh failed: User {Email} is locked out", user.Email);
            return AuthenticationResult.Failed("Account is locked.");
        }

        // Blacklist the old refresh token (rotation strategy)
        await _tokenService.CompromiseRefreshTokenAsync(refreshToken, "token_refresh");

        // Clean up expired compromised tokens during refresh
        await _tokenService.CleanupExpiredCompromisedTokensAsync();

        _logger.LogInformation("Token refreshed successfully for user {Email}", user.Email);
        
        return await GenerateAuthenticationResultAsync(user);
    }

    /// <inheritdoc />
    public async Task<bool> LogoutAsync(string userId, string refreshToken)
    {
        _logger.LogInformation("Logout requested for user: {UserId}", userId);

        // Blacklist the refresh token
        await _tokenService.CompromiseRefreshTokenAsync(refreshToken, "logout");
        
        _logger.LogInformation("User {UserId} logged out successfully", userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RevokeAllTokensAsync(string userId)
    {
        _logger.LogInformation("Revoking all tokens for user: {UserId}", userId);

        // Note: Since we only store compromised tokens (blacklist), 
        // we can't explicitly revoke all tokens without storing active tokens.
        // This would require changing the authentication flow to check 
        // the user's "all tokens revoked after" timestamp.
        
        // For now, we'll update the user's security stamp which will 
        // invalidate all existing tokens when they're validated
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Cannot revoke tokens: User {UserId} not found", userId);
            return false;
        }

        await _userManager.UpdateSecurityStampAsync(user);
        
        _logger.LogInformation("All tokens revoked for user {UserId}", userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult?> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        return new AuthenticationResult
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles.ToList()
        };
    }

    private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiration = _tokenService.GetAccessTokenExpiration();

        return AuthenticationResult.Succeeded(
            accessToken,
            refreshToken,
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToList(),
            expiration
        );
    }
}

