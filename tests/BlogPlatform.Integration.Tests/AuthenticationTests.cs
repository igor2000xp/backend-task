using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Integration.Tests.Helpers;

namespace BlogPlatform.Integration.Tests;

[TestClass]
public class AuthenticationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private AuthTestHelper _authHelper = null!;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [TestInitialize]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _authHelper = new AuthTestHelper(_client);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    #region Registration Tests

    [TestMethod]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            FullName = "New Test User"
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(), JsonOptions);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
        Assert.AreEqual("newuser@test.com", result.Email);
        Assert.IsNotNull(result.Roles);
        Assert.IsTrue(result.Roles.Contains("User"));
    }

    [TestMethod]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange - Register first user
        var firstRequest = new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            FullName = "First User"
        };
        await _client.PostAsync("/api/auth/register", 
            new StringContent(JsonSerializer.Serialize(firstRequest), Encoding.UTF8, "application/json"));

        // Try to register with same email
        var secondRequest = new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Test@67890",
            ConfirmPassword = "Test@67890",
            FullName = "Second User"
        };
        var content = new StringContent(JsonSerializer.Serialize(secondRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "weakpass@test.com",
            Password = "weak",
            ConfirmPassword = "weak",
            FullName = "Weak Pass User"
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "mismatch@test.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@67890",
            FullName = "Mismatch User"
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Tests

    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange - Register user first
        await _authHelper.RegisterAsync("logintest@test.com", "Test@12345", "Login Test User");

        var loginRequest = new LoginRequest
        {
            Email = "logintest@test.com",
            Password = "Test@12345"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(), JsonOptions);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
    }

    [TestMethod]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "Test@12345"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - Register user first
        await _authHelper.RegisterAsync("wrongpass@test.com", "Test@12345", "Wrong Pass User");

        var loginRequest = new LoginRequest
        {
            Email = "wrongpass@test.com",
            Password = "WrongPassword123!"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Token Refresh Tests

    [TestMethod]
    public async Task Refresh_WithValidTokens_ReturnsNewTokens()
    {
        // Arrange - Register and get initial tokens
        var authResult = await _authHelper.RegisterAsync("refresh@test.com", "Test@12345", "Refresh Test User");
        Assert.IsNotNull(authResult);

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = authResult.AccessToken!,
            RefreshToken = authResult.RefreshToken!
        };
        var content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(), JsonOptions);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
        // New tokens should be different from old tokens
        Assert.AreNotEqual(authResult.AccessToken, result.AccessToken);
        Assert.AreNotEqual(authResult.RefreshToken, result.RefreshToken);
    }

    [TestMethod]
    public async Task Refresh_WithUsedRefreshToken_ReturnsUnauthorized()
    {
        // Arrange - Register and get initial tokens
        var authResult = await _authHelper.RegisterAsync("usedtoken@test.com", "Test@12345", "Used Token User");
        Assert.IsNotNull(authResult);

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = authResult.AccessToken!,
            RefreshToken = authResult.RefreshToken!
        };
        var content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

        // First refresh should succeed
        var firstResponse = await _client.PostAsync("/api/auth/refresh", content);
        Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act - Try to use the same refresh token again
        var secondResponse = await _client.PostAsync("/api/auth/refresh", content);

        // Assert - Should fail because the token was already used (blacklisted)
        Assert.AreEqual(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    [TestMethod]
    public async Task Refresh_WithInvalidAccessToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = "invalid.access.token",
            RefreshToken = "someRefreshToken"
        };
        var content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Logout Tests

    [TestMethod]
    public async Task Logout_WithValidToken_BlacklistsRefreshToken()
    {
        // Arrange - Register user
        var authResult = await _authHelper.RegisterAsync("logout@test.com", "Test@12345", "Logout Test User");
        Assert.IsNotNull(authResult);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = authResult.RefreshToken!
        };
        var content = new StringContent(JsonSerializer.Serialize(logoutRequest), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Controller returns Ok with a message
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify refresh token is blacklisted by trying to use it
        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = authResult.AccessToken!,
            RefreshToken = authResult.RefreshToken!
        };
        var refreshContent = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
        var refreshResponse = await _client.PostAsync("/api/auth/refresh", refreshContent);
        
        Assert.AreEqual(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [TestMethod]
    public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "someRefreshToken"
        };
        var content = new StringContent(JsonSerializer.Serialize(logoutRequest), Encoding.UTF8, "application/json");

        // Act - Call without authorization header
        var response = await _client.PostAsync("/api/auth/logout", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Protected Endpoint Tests

    [TestMethod]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsSuccess()
    {
        // Arrange - Register user
        var authResult = await _authHelper.RegisterAsync("protected@test.com", "Test@12345", "Protected Test User");
        Assert.IsNotNull(authResult);

        // Create blog request (requires authentication)
        var blogRequest = new { Name = "Authenticated Blog", IsActive = true };
        var content = new StringContent(JsonSerializer.Serialize(blogRequest), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange - Create blog request without auth
        var blogRequest = new { Name = "Unauthenticated Blog", IsActive = true };
        var content = new StringContent(JsonSerializer.Serialize(blogRequest), Encoding.UTF8, "application/json");

        // Act - Make request without authorization header
        var response = await _client.PostAsync("/api/blogs", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange - Create blog request with invalid token
        var blogRequest = new { Name = "Invalid Token Blog", IsActive = true };
        var content = new StringContent(JsonSerializer.Serialize(blogRequest), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task PublicEndpoint_WithoutToken_ReturnsSuccess()
    {
        // Act - Get blogs (public endpoint)
        var response = await _client.GetAsync("/api/blogs");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}

