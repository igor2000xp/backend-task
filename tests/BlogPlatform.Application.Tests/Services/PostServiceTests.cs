using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.Services;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;
using Moq;

namespace BlogPlatform.Application.Tests.Services;

[TestClass]
public class PostServiceTests
{
    private Mock<IPostRepository> _mockPostRepository = null!;
    private Mock<IBlogRepository> _mockBlogRepository = null!;
    private PostService _service = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _mockPostRepository = new Mock<IPostRepository>();
        _mockBlogRepository = new Mock<IBlogRepository>();
        _service = new PostService(_mockPostRepository.Object, _mockBlogRepository.Object);
    }
    
    [TestMethod]
    public async Task CreatePostAsync_ValidRequest_ShouldReturnDto()
    {
        // Arrange
        var userId = "test-user-id";
        var blog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        var request = new CreatePostRequest 
        { 
            Name = "Test Post Name", 
            Content = "Test content for the post",
            BlogId = 1 
        };
        var expectedPost = new PostEntity 
        { 
            PostId = 1, 
            Name = "Test Post Name", 
            Content = "Test content for the post",
            ParentId = 1,
            Created = DateTime.UtcNow,
            Blog = blog,
            UserId = userId
        };
        
        _mockBlogRepository.Setup(r => r.GetByIdAsync(1))
                           .ReturnsAsync(blog);
        _mockPostRepository.Setup(r => r.CreateAsync(It.IsAny<PostEntity>()))
                           .ReturnsAsync(expectedPost);
        
        // Act
        var result = await _service.CreatePostAsync(request, userId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test Post Name", result.Name);
        Assert.AreEqual("Test content for the post", result.Content);
        Assert.AreEqual(userId, result.UserId);
        _mockPostRepository.Verify(r => r.CreateAsync(It.IsAny<PostEntity>()), Times.Once);
    }

    [TestMethod]
    public async Task CreatePostAsync_NonExistingBlog_ShouldThrowException()
    {
        // Arrange
        var request = new CreatePostRequest 
        { 
            Name = "Test Post Name", 
            Content = "Test content",
            BlogId = 999 
        };
        
        _mockBlogRepository.Setup(r => r.GetByIdAsync(999))
                           .ReturnsAsync((BlogEntity?)null);
        
        // Act & Assert
        try
        {
            await _service.CreatePostAsync(request, "user-id");
            Assert.Fail("Expected KeyNotFoundException was not thrown");
        }
        catch (KeyNotFoundException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    [TestMethod]
    public async Task GetPostByIdAsync_ExistingPost_ShouldReturnDto()
    {
        // Arrange
        var blog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        var post = new PostEntity 
        { 
            PostId = 1, 
            Name = "Test Post Name", 
            Content = "Test content",
            ParentId = 1,
            Created = DateTime.UtcNow,
            Blog = blog
        };
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(1))
                           .ReturnsAsync(post);
        
        // Act
        var result = await _service.GetPostByIdAsync(1);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test Post Name", result.Name);
        Assert.AreEqual("Test Blog Name", result.BlogName);
    }

    [TestMethod]
    public async Task GetPostByIdAsync_NonExistingPost_ShouldReturnNull()
    {
        // Arrange
        _mockPostRepository.Setup(r => r.GetByIdAsync(999))
                           .ReturnsAsync((PostEntity?)null);
        
        // Act
        var result = await _service.GetPostByIdAsync(999);
        
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllPostsAsync_MultiplePosts_ShouldReturnDtos()
    {
        // Arrange
        var blog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        var posts = new List<PostEntity>
        {
            new PostEntity { PostId = 1, Name = "First Post Name", Content = "Content 1", ParentId = 1, Created = DateTime.UtcNow, Blog = blog },
            new PostEntity { PostId = 2, Name = "Second Post Name", Content = "Content 2", ParentId = 1, Created = DateTime.UtcNow, Blog = blog }
        };
        
        _mockPostRepository.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(posts);
        
        // Act
        var result = await _service.GetAllPostsAsync();
        
        // Assert
        var dtos = result.ToList();
        Assert.AreEqual(2, dtos.Count);
        Assert.AreEqual("First Post Name", dtos[0].Name);
        Assert.AreEqual("Second Post Name", dtos[1].Name);
    }

    [TestMethod]
    public async Task GetPostsByBlogIdAsync_MultiplePosts_ShouldReturnOnlyBlogPosts()
    {
        // Arrange
        var blog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        var posts = new List<PostEntity>
        {
            new PostEntity { PostId = 1, Name = "Post 1 Name", Content = "Content 1", ParentId = 1, Created = DateTime.UtcNow, Blog = blog },
            new PostEntity { PostId = 2, Name = "Post 2 Name", Content = "Content 2", ParentId = 1, Created = DateTime.UtcNow, Blog = blog }
        };
        
        _mockPostRepository.Setup(r => r.GetByBlogIdAsync(1))
                           .ReturnsAsync(posts);
        
        // Act
        var result = await _service.GetPostsByBlogIdAsync(1);
        
        // Assert
        var dtos = result.ToList();
        Assert.AreEqual(2, dtos.Count);
        Assert.IsTrue(dtos.All(p => p.BlogId == 1));
    }

    [TestMethod]
    public async Task UpdatePostAsync_ExistingPost_ShouldUpdateProperties()
    {
        // Arrange
        var userId = "owner-id";
        var blog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        var post = new PostEntity 
        { 
            PostId = 1, 
            Name = "Original Name", 
            Content = "Original content",
            ParentId = 1,
            Created = DateTime.UtcNow,
            Blog = blog,
            UserId = userId
        };
        var request = new UpdatePostRequest 
        { 
            Name = "Updated Post Name", 
            Content = "Updated content" 
        };
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(1))
                           .ReturnsAsync(post);
        _mockPostRepository.Setup(r => r.UpdateAsync(It.IsAny<PostEntity>()))
                           .Returns(Task.CompletedTask);
        
        // Act
        await _service.UpdatePostAsync(1, request, userId, isAdmin: false);
        
        // Assert
        Assert.AreEqual("Updated Post Name", post.Name);
        Assert.AreEqual("Updated content", post.Content);
        _mockPostRepository.Verify(r => r.UpdateAsync(It.IsAny<PostEntity>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdatePostAsync_NonExistingPost_ShouldThrowException()
    {
        // Arrange
        var request = new UpdatePostRequest { Name = "Updated Name", Content = "Updated content" };
        _mockPostRepository.Setup(r => r.GetByIdAsync(999))
                           .ReturnsAsync((PostEntity?)null);
        
        // Act & Assert
        try
        {
            await _service.UpdatePostAsync(999, request, "any-user", false);
            Assert.Fail("Expected KeyNotFoundException was not thrown");
        }
        catch (KeyNotFoundException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    [TestMethod]
    public async Task DeletePostAsync_Owner_ShouldCallRepository()
    {
        // Arrange
        var userId = "owner-id";
        var post = new PostEntity { PostId = 1, UserId = userId };
        _mockPostRepository.Setup(r => r.GetByIdAsync(1))
                           .ReturnsAsync(post);
        _mockPostRepository.Setup(r => r.DeleteAsync(1))
                           .Returns(Task.CompletedTask);
        
        // Act
        await _service.DeletePostAsync(1, userId, isAdmin: false);
        
        // Assert
        _mockPostRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}
