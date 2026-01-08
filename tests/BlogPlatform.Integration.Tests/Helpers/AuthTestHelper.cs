using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlogPlatform.Application.DTOs.Auth;

namespace BlogPlatform.Integration.Tests.Helpers;

/// <summary>
/// Helper class for authentication in integration tests
/// </summary>
public class AuthTestHelper
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthTestHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Registers a new user and returns the authentication result
    /// </summary>
    public async Task<AuthenticationResult?> RegisterAsync(string email, string password, string fullName)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FullName = fullName
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
    }

    /// <summary>
    /// Logs in a user and returns the authentication result
    /// </summary>
    public async Task<AuthenticationResult?> LoginAsync(string email, string password)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
    }

    /// <summary>
    /// Refreshes tokens and returns the new authentication result
    /// </summary>
    public async Task<AuthenticationResult?> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var request = new RefreshTokenRequest
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/refresh", content);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
    }

    /// <summary>
    /// Logs out a user
    /// </summary>
    public async Task<bool> LogoutAsync(string accessToken, string refreshToken)
    {
        var request = new LogoutRequest
        {
            RefreshToken = refreshToken
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = content
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(requestMessage);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Sets authorization header for client
    /// </summary>
    public void SetAuthorizationHeader(string accessToken)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Clears authorization header
    /// </summary>
    public void ClearAuthorizationHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Gets a pre-seeded admin user credentials
    /// </summary>
    public static (string Email, string Password) GetAdminCredentials()
    {
        return ("admin@blogplatform.com", "Admin@123456");
    }

    /// <summary>
    /// Gets a pre-seeded user credentials
    /// </summary>
    public static (string Email, string Password) GetUserCredentials()
    {
        return ("user@blogplatform.com", "User@123456");
    }
}

