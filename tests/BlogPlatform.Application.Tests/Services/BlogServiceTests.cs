using System.ComponentModel.DataAnnotations;
using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.Services;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Interfaces;
using Moq;

namespace BlogPlatform.Application.Tests.Services;

[TestClass]
public class BlogServiceTests
{
    private Mock<IBlogRepository> _mockRepository = null!;
    private BlogService _service = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IBlogRepository>();
        _service = new BlogService(_mockRepository.Object);
    }
    
    [TestMethod]
    public async Task CreateBlogAsync_ValidRequest_ShouldReturnDto()
    {
        // Arrange
        var request = new CreateBlogRequest { Name = "Test Blog Name", IsActive = true };
        var expectedBlog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<BlogEntity>()))
                       .ReturnsAsync(expectedBlog);
        
        // Act
        var result = await _service.CreateBlogAsync(request);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test Blog Name", result.Name);
        Assert.IsTrue(result.IsActive);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<BlogEntity>()), Times.Once);
    }

    [TestMethod]
    public async Task GetBlogByIdAsync_ExistingBlog_ShouldReturnDto()
    {
        // Arrange
        var blog = new BlogEntity { BlogId = 1, Name = "Test Blog Name", IsActive = true };
        _mockRepository.Setup(r => r.GetByIdAsync(1))
                       .ReturnsAsync(blog);
        
        // Act
        var result = await _service.GetBlogByIdAsync(1);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test Blog Name", result.Name);
    }

    [TestMethod]
    public async Task GetBlogByIdAsync_NonExistingBlog_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
                       .ReturnsAsync((BlogEntity?)null);
        
        // Act
        var result = await _service.GetBlogByIdAsync(999);
        
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllBlogsAsync_MultipleBlogs_ShouldReturnDtos()
    {
        // Arrange
        var blogs = new List<BlogEntity>
        {
            new BlogEntity { BlogId = 1, Name = "First Blog Name", IsActive = true },
            new BlogEntity { BlogId = 2, Name = "Second Blog Name", IsActive = false }
        };
        _mockRepository.Setup(r => r.GetAllAsync())
                       .ReturnsAsync(blogs);
        
        // Act
        var result = await _service.GetAllBlogsAsync();
        
        // Assert
        var dtos = result.ToList();
        Assert.AreEqual(2, dtos.Count);
        Assert.AreEqual("First Blog Name", dtos[0].Name);
        Assert.AreEqual("Second Blog Name", dtos[1].Name);
    }

    [TestMethod]
    public async Task UpdateBlogAsync_ExistingBlog_ShouldUpdateProperties()
    {
        // Arrange
        var blog = new BlogEntity { BlogId = 1, Name = "Original Name", IsActive = true };
        var request = new UpdateBlogRequest { Name = "Updated Blog Name", IsActive = false };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1))
                       .ReturnsAsync(blog);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<BlogEntity>()))
                       .Returns(Task.CompletedTask);
        
        // Act
        await _service.UpdateBlogAsync(1, request);
        
        // Assert
        Assert.AreEqual("Updated Blog Name", blog.Name);
        Assert.IsFalse(blog.IsActive);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<BlogEntity>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateBlogAsync_NonExistingBlog_ShouldThrowException()
    {
        // Arrange
        var request = new UpdateBlogRequest { Name = "Updated Name", IsActive = false };
        _mockRepository.Setup(r => r.GetByIdAsync(999))
                       .ReturnsAsync((BlogEntity?)null);
        
        // Act & Assert
        try
        {
            await _service.UpdateBlogAsync(999, request);
            Assert.Fail("Expected KeyNotFoundException was not thrown");
        }
        catch (KeyNotFoundException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    [TestMethod]
    public async Task DeleteBlogAsync_ShouldCallRepository()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1))
                       .Returns(Task.CompletedTask);
        
        // Act
        await _service.DeleteBlogAsync(1);
        
        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task GetBlogByIdAsync_WithArticles_ShouldIncludeArticleCount()
    {
        // Arrange
        var blog = new BlogEntity 
        { 
            BlogId = 1, 
            Name = "Test Blog Name", 
            IsActive = true,
            Articles = new List<PostEntity>
            {
                new PostEntity { PostId = 1, Name = "Post 1 Name", Content = "Content 1", ParentId = 1, Created = DateTime.UtcNow },
                new PostEntity { PostId = 2, Name = "Post 2 Name", Content = "Content 2", ParentId = 1, Created = DateTime.UtcNow }
            }
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1))
                       .ReturnsAsync(blog);
        
        // Act
        var result = await _service.GetBlogByIdAsync(1);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ArticleCount);
    }
}

