using System.Net;
using System.Text;
using System.Text.Json;
using BlogPlatform.Application.DTOs;
using BlogPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Integration.Tests;

[TestClass]
public class BlogEndToEndTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove all DbContext-related service descriptors
                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<BlogsContext>) ||
                                    d.ServiceType == typeof(DbContextOptions) ||
                                    d.ServiceType == typeof(BlogsContext))
                        .ToList();
                    
                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }
                    
                    // Add InMemory database for integration tests
                    services.AddDbContext<BlogsContext>(options =>
                    {
                        options.UseInMemoryDatabase($"IntegrationTests_{Guid.NewGuid()}");
                    }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
                });
            });
        
        _client = _factory.CreateClient();
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
        var request = new CreateBlogRequest { Name = "Integration Test Blog", IsActive = true };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/blogs", content);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var blogDto = await JsonSerializer.DeserializeAsync<BlogDto>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(blogDto);
        Assert.AreEqual("Integration Test Blog", blogDto.Name);
        Assert.IsTrue(blogDto.IsActive);
    }
    
    [TestMethod]
    public async Task CreateBlog_InvalidName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateBlogRequest { Name = "Bad", IsActive = true };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/blogs", content);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task GetBlogs_AfterCreating_ShouldReturnBlogs()
    {
        // Arrange - Create a blog first
        var createRequest = new CreateBlogRequest { Name = "Test Blog Name", IsActive = true };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json");
        await _client.PostAsync("/api/blogs", createContent);
        
        // Act
        var response = await _client.GetAsync("/api/blogs");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var blogs = await JsonSerializer.DeserializeAsync<List<BlogDto>>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(blogs);
        Assert.IsTrue(blogs.Count > 0);
    }
    
    [TestMethod]
    public async Task GetBlogById_ExistingBlog_ShouldReturnBlog()
    {
        // Arrange - Create a blog first
        var createRequest = new CreateBlogRequest { Name = "Test Blog Name", IsActive = true };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/blogs", createContent);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Act
        var response = await _client.GetAsync($"/api/blogs/{createdBlog!.Id}");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
        var createRequest = new CreateBlogRequest { Name = "Original Name", IsActive = true };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/blogs", createContent);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var updateRequest = new UpdateBlogRequest { Name = "Updated Blog Name", IsActive = false };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PutAsync($"/api/blogs/{createdBlog!.Id}", updateContent);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task DeleteBlog_ExistingBlog_ShouldReturnNoContent()
    {
        // Arrange - Create a blog first
        var createRequest = new CreateBlogRequest { Name = "Test Blog Name", IsActive = true };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/blogs", createContent);
        var createdBlog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await createResponse.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Act
        var response = await _client.DeleteAsync($"/api/blogs/{createdBlog!.Id}");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task CreatePostForBlog_ValidData_ShouldLinkCorrectly()
    {
        // Arrange - Create blog first
        var blogRequest = new CreateBlogRequest { Name = "Blog For Posts", IsActive = true };
        var blogContent = new StringContent(
            JsonSerializer.Serialize(blogRequest),
            Encoding.UTF8,
            "application/json");
        var blogResponse = await _client.PostAsync("/api/blogs", blogContent);
        var blog = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blogResponse.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Act - Create post
        var postRequest = new CreatePostRequest
        {
            Name = "Test Post Name",
            Content = "This is test content for the post.",
            BlogId = blog!.Id
        };
        var postContent = new StringContent(
            JsonSerializer.Serialize(postRequest),
            Encoding.UTF8,
            "application/json");
        var postResponse = await _client.PostAsync("/api/posts", postContent);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, postResponse.StatusCode);
        var post = await JsonSerializer.DeserializeAsync<PostDto>(
            await postResponse.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(post);
        Assert.AreEqual(blog.Id, post.BlogId);
    }
    
    [TestMethod]
    public async Task CreatePost_NonExistingBlog_ShouldReturnNotFound()
    {
        // Arrange
        var postRequest = new CreatePostRequest
        {
            Name = "Test Post Name",
            Content = "Test content",
            BlogId = 999
        };
        var postContent = new StringContent(
            JsonSerializer.Serialize(postRequest),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var postResponse = await _client.PostAsync("/api/posts", postContent);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, postResponse.StatusCode);
    }
    
    [TestMethod]
    public async Task GetPostsByBlogId_MultiplePost_ShouldReturnOnlyBlogPosts()
    {
        // Arrange - Create two blogs with posts
        var blog1Request = new CreateBlogRequest { Name = "First Blog Name", IsActive = true };
        var blog1Content = new StringContent(
            JsonSerializer.Serialize(blog1Request),
            Encoding.UTF8,
            "application/json");
        var blog1Response = await _client.PostAsync("/api/blogs", blog1Content);
        var blog1 = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blog1Response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var blog2Request = new CreateBlogRequest { Name = "Second Blog Name", IsActive = true };
        var blog2Content = new StringContent(
            JsonSerializer.Serialize(blog2Request),
            Encoding.UTF8,
            "application/json");
        var blog2Response = await _client.PostAsync("/api/blogs", blog2Content);
        var blog2 = await JsonSerializer.DeserializeAsync<BlogDto>(
            await blog2Response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Create posts for blog1
        var post1Request = new CreatePostRequest { Name = "Post 1 Blog 1", Content = "Content", BlogId = blog1!.Id };
        await _client.PostAsync("/api/posts", new StringContent(
            JsonSerializer.Serialize(post1Request), Encoding.UTF8, "application/json"));
        
        var post2Request = new CreatePostRequest { Name = "Post 2 Blog 1", Content = "Content", BlogId = blog1.Id };
        await _client.PostAsync("/api/posts", new StringContent(
            JsonSerializer.Serialize(post2Request), Encoding.UTF8, "application/json"));
        
        // Create post for blog2
        var post3Request = new CreatePostRequest { Name = "Post 1 Blog 2", Content = "Content", BlogId = blog2!.Id };
        await _client.PostAsync("/api/posts", new StringContent(
            JsonSerializer.Serialize(post3Request), Encoding.UTF8, "application/json"));
        
        // Act
        var response = await _client.GetAsync($"/api/posts/blog/{blog1.Id}");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var posts = await JsonSerializer.DeserializeAsync<List<PostDto>>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(posts);
        Assert.AreEqual(2, posts.Count);
        Assert.IsTrue(posts.All(p => p.BlogId == blog1.Id));
    }
}

