using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Integration.Tests.Helpers;

namespace BlogPlatform.Integration.Tests;

[TestClass]
public class BlogEndToEndTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private AuthTestHelper _authHelper = null!;
    private string? _accessToken;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    
    [TestInitialize]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _authHelper = new AuthTestHelper(_client);
        
        // Register and get token for authenticated tests
        var authResult = await _authHelper.RegisterAsync($"testuser_{Guid.NewGuid()}@test.com", "Test@12345", "Test User");
        _accessToken = authResult?.AccessToken;
    }
    
    private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string uri, object? content = null)
    {
        var request = new HttpRequestMessage(method, uri);
        if (_accessToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
        if (content != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        }
        return request;
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [TestMethod]
    public async Task CreateBlog_ValidData_ShouldReturnCreated()
    {
        // Arrange
        var blogData = new { Name = "Integration Test Blog", IsActive = true };
        var request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var blogDto = await JsonSerializer.DeserializeAsync<BlogDto>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
        Assert.IsNotNull(blogDto);
        Assert.AreEqual("Integration Test Blog", blogDto.Name);
        Assert.IsTrue(blogDto.IsActive);
    }
    
    [TestMethod]
    public async Task CreateBlog_InvalidName_ShouldReturnBadRequest()
    {
        // Arrange
        var blogData = new { Name = "Bad", IsActive = true };
        var request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task GetBlogs_AfterCreating_ShouldReturnBlogs()
    {
        // Arrange - Create a blog first
        var blogData = new { Name = "Test Blog Name", IsActive = true };
        var createRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        await _client.SendAsync(createRequest);
        
        // Act
        var response = await _client.GetAsync("/api/blogs");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var blogs = await JsonSerializer.DeserializeAsync<List<BlogDto>>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
        Assert.IsNotNull(blogs);
        Assert.IsTrue(blogs.Count > 0);
    }
    
    [TestMethod]
    public async Task GetBlogById_ExistingBlog_ShouldReturnBlog()
    {
        // Arrange - Create a blog first
        var blogData = new { Name = "Test Blog Name", IsActive = true };
        var createRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(),
            JsonOptions);
        
        // Act
        var response = await _client.GetAsync($"/api/blogs/{createdBlog!.Id}");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
        Assert.IsNotNull(blog);
        Assert.AreEqual(createdBlog.Id, blog.Id);
    }
    
    [TestMethod]
    public async Task GetBlogById_NonExisting_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/blogs/999");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task UpdateBlog_ValidData_ShouldReturnNoContent()
    {
        // Arrange - Create a blog first
        var blogData = new { Name = "Original Name", IsActive = true };
        var createRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(),
            JsonOptions);
        
        var updateData = new { Name = "Updated Blog Name", IsActive = false };
        var updateRequest = CreateAuthenticatedRequest(HttpMethod.Put, $"/api/blogs/{createdBlog!.Id}", updateData);
        
        // Act
        var response = await _client.SendAsync(updateRequest);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task DeleteBlog_ExistingBlog_ShouldReturnNoContent()
    {
        // Arrange - Create a blog first
        var blogData = new { Name = "Test Blog Name", IsActive = true };
        var createRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        var createResponse = await _client.SendAsync(createRequest);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(),
            JsonOptions);
        
        // Act
        var deleteRequest = CreateAuthenticatedRequest(HttpMethod.Delete, $"/api/blogs/{createdBlog!.Id}");
        var response = await _client.SendAsync(deleteRequest);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task CreatePostForBlog_ValidData_ShouldLinkCorrectly()
    {
        // Arrange - Create blog first
        var blogData = new { Name = "Blog For Posts", IsActive = true };
        var blogRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blogData);
        var blogResponse = await _client.SendAsync(blogRequest);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blogResponse.Content.ReadAsStreamAsync(),
            JsonOptions);
        
        // Act - Create post
        var postData = new
        {
            Name = "Test Post Name",
            Content = "This is test content for the post.",
            BlogId = blog!.Id
        };
        var postRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/posts", postData);
        var postResponse = await _client.SendAsync(postRequest);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, postResponse.StatusCode);
        var post = await JsonSerializer.DeserializeAsync<PostDto>(
            await postResponse.Content.ReadAsStreamAsync(),
            JsonOptions);
        Assert.IsNotNull(post);
        Assert.AreEqual(blog.Id, post.BlogId);
    }
    
    [TestMethod]
    public async Task CreatePost_NonExistingBlog_ShouldReturnNotFound()
    {
        // Arrange
        var postData = new
        {
            Name = "Test Post Name",
            Content = "Test content",
            BlogId = 999
        };
        var postRequest = CreateAuthenticatedRequest(HttpMethod.Post, "/api/posts", postData);
        
        // Act
        var postResponse = await _client.SendAsync(postRequest);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, postResponse.StatusCode);
    }
    
    [TestMethod]
    public async Task GetPostsByBlogId_MultiplePost_ShouldReturnOnlyBlogPosts()
    {
        // Arrange - Create two blogs with posts
        var blog1Data = new { Name = "First Blog Name", IsActive = true };
        var blog1Request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blog1Data);
        var blog1Response = await _client.SendAsync(blog1Request);
        var blog1 = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blog1Response.Content.ReadAsStreamAsync(),
            JsonOptions);
        
        var blog2Data = new { Name = "Second Blog Name", IsActive = true };
        var blog2Request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/blogs", blog2Data);
        var blog2Response = await _client.SendAsync(blog2Request);
        var blog2 = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blog2Response.Content.ReadAsStreamAsync(),
            JsonOptions);
        
        // Create posts for blog1
        var post1Data = new { Name = "Post 1 Blog 1", Content = "Content", BlogId = blog1!.Id };
        var post1Request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/posts", post1Data);
        await _client.SendAsync(post1Request);
        
        var post2Data = new { Name = "Post 2 Blog 1", Content = "Content", BlogId = blog1.Id };
        var post2Request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/posts", post2Data);
        await _client.SendAsync(post2Request);
        
        // Create post for blog2
        var post3Data = new { Name = "Post 1 Blog 2", Content = "Content", BlogId = blog2!.Id };
        var post3Request = CreateAuthenticatedRequest(HttpMethod.Post, "/api/posts", post3Data);
        await _client.SendAsync(post3Request);
        
        // Act
        var response = await _client.GetAsync($"/api/posts/blog/{blog1.Id}");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var posts = await JsonSerializer.DeserializeAsync<List<PostDto>>(
            await response.Content.ReadAsStreamAsync(),
            JsonOptions);
        Assert.IsNotNull(posts);
        Assert.AreEqual(2, posts.Count);
        Assert.IsTrue(posts.All(p => p.BlogId == blog1.Id));
    }
}

