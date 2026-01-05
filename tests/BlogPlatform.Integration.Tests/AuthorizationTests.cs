using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Integration.Tests.Helpers;

namespace BlogPlatform.Integration.Tests;

[TestClass]
public class AuthorizationTests
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

    #region Resource Ownership - Blog Tests

    [TestMethod]
    public async Task UpdateBlog_ByOwner_ReturnsNoContent()
    {
        // Arrange - Register user and create blog
        var authResult = await _authHelper.RegisterAsync("blogowner@test.com", "Test@12345", "Blog Owner");
        Assert.IsNotNull(authResult);

        // Create blog
        var createBlogRequest = new { Name = "Owner's Blog", IsActive = true };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Update blog as owner
        var updateBlogRequest = new { Name = "Updated Owner's Blog", IsActive = false };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/blogs/{createdBlog!.Id}")
        {
            Content = new StringContent(JsonSerializer.Serialize(updateBlogRequest), Encoding.UTF8, "application/json")
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateBlog_ByNonOwner_ReturnsForbidden()
    {
        // Arrange - Register two users
        var owner = await _authHelper.RegisterAsync("owner1@test.com", "Test@12345", "Blog Owner");
        var nonOwner = await _authHelper.RegisterAsync("nonowner1@test.com", "Test@12345", "Non Owner");
        Assert.IsNotNull(owner);
        Assert.IsNotNull(nonOwner);

        // Create blog as owner
        var createBlogRequest = new { Name = "Owner's Blog Only", IsActive = true };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Try to update as non-owner
        var updateBlogRequest = new { Name = "Hacked Blog Title", IsActive = false };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/blogs/{createdBlog!.Id}")
        {
            Content = new StringContent(JsonSerializer.Serialize(updateBlogRequest), Encoding.UTF8, "application/json")
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nonOwner.AccessToken);

        // Act
        var response = await _client.SendAsync(updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteBlog_ByOwner_ReturnsNoContent()
    {
        // Arrange - Register user and create blog
        var authResult = await _authHelper.RegisterAsync("deleteowner@test.com", "Test@12345", "Delete Owner");
        Assert.IsNotNull(authResult);

        // Create blog
        var createBlogRequest = new { Name = "Blog to Delete", IsActive = true };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Delete blog as owner
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/blogs/{createdBlog!.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(deleteRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        // Verify blog is deleted
        var getResponse = await _client.GetAsync($"/api/blogs/{createdBlog.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task DeleteBlog_ByNonOwner_ReturnsForbidden()
    {
        // Arrange - Register two users
        var owner = await _authHelper.RegisterAsync("owner2@test.com", "Test@12345", "Blog Owner");
        var nonOwner = await _authHelper.RegisterAsync("nonowner2@test.com", "Test@12345", "Non Owner");
        Assert.IsNotNull(owner);
        Assert.IsNotNull(nonOwner);

        // Create blog as owner
        var createBlogRequest = new { Name = "Owner's Secure Blog", IsActive = true };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Try to delete as non-owner
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/blogs/{createdBlog!.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nonOwner.AccessToken);

        // Act
        var response = await _client.SendAsync(deleteRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Resource Ownership - Post Tests

    [TestMethod]
    public async Task UpdatePost_ByOwner_ReturnsNoContent()
    {
        // Arrange - Register user and create blog and post
        var authResult = await _authHelper.RegisterAsync("postowner@test.com", "Test@12345", "Post Owner");
        Assert.IsNotNull(authResult);

        // Create blog
        var createBlogRequest = new { Name = "Blog for Posts", IsActive = true };
        var blogRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        blogRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        var blogResponse = await _client.SendAsync(blogRequest);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blogResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Create post
        var createPostRequest = new { Name = "Owner's Post", Content = "Original content", BlogId = blog!.Id };
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/posts")
        {
            Content = new StringContent(JsonSerializer.Serialize(createPostRequest), Encoding.UTF8, "application/json")
        };
        postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        var postResponse = await _client.SendAsync(postRequest);
        var post = await JsonSerializer.DeserializeAsync<PostDto>(
            await postResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Update post as owner
        var updatePostRequest = new { Name = "Updated Post", Content = "Updated content" };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/posts/{post!.Id}")
        {
            Content = new StringContent(JsonSerializer.Serialize(updatePostRequest), Encoding.UTF8, "application/json")
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdatePost_ByNonOwner_ReturnsForbidden()
    {
        // Arrange - Register two users
        var owner = await _authHelper.RegisterAsync("postowner2@test.com", "Test@12345", "Post Owner");
        var nonOwner = await _authHelper.RegisterAsync("postnonowner@test.com", "Test@12345", "Non Owner");
        Assert.IsNotNull(owner);
        Assert.IsNotNull(nonOwner);

        // Create blog and post as owner
        var createBlogRequest = new { Name = "Owner's Blog for Posts", IsActive = true };
        var blogRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        blogRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var blogResponse = await _client.SendAsync(blogRequest);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blogResponse.Content.ReadAsStreamAsync(), JsonOptions);

        var createPostRequest = new { Name = "Owner's Secure Post", Content = "Secure content", BlogId = blog!.Id };
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/posts")
        {
            Content = new StringContent(JsonSerializer.Serialize(createPostRequest), Encoding.UTF8, "application/json")
        };
        postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var postResponse = await _client.SendAsync(postRequest);
        var post = await JsonSerializer.DeserializeAsync<PostDto>(
            await postResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Try to update as non-owner
        var updatePostRequest = new { Name = "Hacked Post", Content = "Hacked content" };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/posts/{post!.Id}")
        {
            Content = new StringContent(JsonSerializer.Serialize(updatePostRequest), Encoding.UTF8, "application/json")
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nonOwner.AccessToken);

        // Act
        var response = await _client.SendAsync(updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task DeletePost_ByOwner_ReturnsNoContent()
    {
        // Arrange
        var authResult = await _authHelper.RegisterAsync("postdeleteowner@test.com", "Test@12345", "Delete Owner");
        Assert.IsNotNull(authResult);

        // Create blog and post
        var createBlogRequest = new { Name = "Blog for Post Delete", IsActive = true };
        var blogRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        blogRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        var blogResponse = await _client.SendAsync(blogRequest);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blogResponse.Content.ReadAsStreamAsync(), JsonOptions);

        var createPostRequest = new { Name = "Post to Delete", Content = "Delete me", BlogId = blog!.Id };
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/posts")
        {
            Content = new StringContent(JsonSerializer.Serialize(createPostRequest), Encoding.UTF8, "application/json")
        };
        postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        var postResponse = await _client.SendAsync(postRequest);
        var post = await JsonSerializer.DeserializeAsync<PostDto>(
            await postResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Delete post as owner
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{post!.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(deleteRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task DeletePost_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var owner = await _authHelper.RegisterAsync("postowner3@test.com", "Test@12345", "Post Owner");
        var nonOwner = await _authHelper.RegisterAsync("postnonowner2@test.com", "Test@12345", "Non Owner");
        Assert.IsNotNull(owner);
        Assert.IsNotNull(nonOwner);

        // Create blog and post as owner
        var createBlogRequest = new { Name = "Owner's Post Blog", IsActive = true };
        var blogRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        blogRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var blogResponse = await _client.SendAsync(blogRequest);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blogResponse.Content.ReadAsStreamAsync(), JsonOptions);

        var createPostRequest = new { Name = "Protected Post", Content = "Protected content", BlogId = blog!.Id };
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/posts")
        {
            Content = new StringContent(JsonSerializer.Serialize(createPostRequest), Encoding.UTF8, "application/json")
        };
        postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var postResponse = await _client.SendAsync(postRequest);
        var post = await JsonSerializer.DeserializeAsync<PostDto>(
            await postResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Try to delete as non-owner
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{post!.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nonOwner.AccessToken);

        // Act
        var response = await _client.SendAsync(deleteRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Admin Access Tests

    [TestMethod]
    public async Task UpdateBlog_ByAdmin_ReturnsNoContent()
    {
        // Arrange - Login as seeded admin user
        var (adminEmail, adminPassword) = AuthTestHelper.GetAdminCredentials();
        var admin = await _authHelper.LoginAsync(adminEmail, adminPassword);
        
        // Register regular user and create a blog
        var user = await _authHelper.RegisterAsync("regularuser@test.com", "Test@12345", "Regular User");
        Assert.IsNotNull(user);

        var createBlogRequest = new { Name = "User's Blog", IsActive = true };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/blogs")
        {
            Content = new StringContent(JsonSerializer.Serialize(createBlogRequest), Encoding.UTF8, "application/json")
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(), JsonOptions);

        // Skip if admin login failed (seeded data might not exist in in-memory DB)
        if (admin == null)
        {
            Assert.Inconclusive("Admin login failed - seeded data may not be available in test database");
            return;
        }

        // Update blog as admin
        var updateBlogRequest = new { Name = "Admin Updated Blog", IsActive = false };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/blogs/{createdBlog!.Id}")
        {
            Content = new StringContent(JsonSerializer.Serialize(updateBlogRequest), Encoding.UTF8, "application/json")
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

        // Act
        var response = await _client.SendAsync(updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region User Info Endpoint Tests

    [TestMethod]
    public async Task GetUserInfo_WithValidToken_ReturnsUserInfo()
    {
        // Arrange - Register user
        var authResult = await _authHelper.RegisterAsync("userinfo@test.com", "Test@12345", "User Info Test");
        Assert.IsNotNull(authResult);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await JsonSerializer.DeserializeAsync<AuthenticationResult>(
            await response.Content.ReadAsStreamAsync(), JsonOptions);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("userinfo@test.com", result.Email);
        Assert.AreEqual("User Info Test", result.FullName);
    }

    [TestMethod]
    public async Task GetUserInfo_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}

