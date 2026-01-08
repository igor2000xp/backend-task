using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Tests.Repositories;

[TestClass]
public class BlogRepositoryTests
{
    [TestMethod]
    public async Task CreateAsync_ValidBlog_ShouldReturnBlogWithId()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        
        // Act
        var result = await repository.CreateAsync(blog);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.BlogId > 0);
        Assert.AreEqual("Test Blog Name", result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_ExistingBlog_ShouldReturnBlog()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        await repository.CreateAsync(blog);
        
        // Act
        var result = await repository.GetByIdAsync(blog.BlogId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(blog.BlogId, result.BlogId);
        Assert.AreEqual("Test Blog Name", result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_NonExistingBlog_ShouldReturnNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        
        // Act
        var result = await repository.GetByIdAsync(999);
        
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllAsync_MultipleBlogs_ShouldReturnAll()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        
        await repository.CreateAsync(new BlogEntity { Name = "First Blog Name", IsActive = true });
        await repository.CreateAsync(new BlogEntity { Name = "Second Blog Name", IsActive = false });
        
        // Act
        var result = await repository.GetAllAsync();
        
        // Assert
        var blogs = result.ToList();
        Assert.AreEqual(2, blogs.Count);
    }

    [TestMethod]
    public async Task UpdateAsync_ExistingBlog_ShouldUpdateProperties()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        
        var blog = new BlogEntity { Name = "Original Name", IsActive = true };
        await repository.CreateAsync(blog);
        
        // Act
        blog.Name = "Updated Blog Name";
        blog.IsActive = false;
        await repository.UpdateAsync(blog);
        
        // Assert
        var updated = await repository.GetByIdAsync(blog.BlogId);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Blog Name", updated.Name);
        Assert.IsFalse(updated.IsActive);
    }

    [TestMethod]
    public async Task DeleteAsync_ExistingBlog_ShouldRemoveBlog()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        await repository.CreateAsync(blog);
        
        // Act
        await repository.DeleteAsync(blog.BlogId);
        
        // Assert
        var deleted = await repository.GetByIdAsync(blog.BlogId);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithArticles_ShouldIncludeArticles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var repository = new BlogRepository(context);
        
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        await repository.CreateAsync(blog);
        
        var post = new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content",
            ParentId = blog.BlogId,
            Created = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetByIdAsync(blog.BlogId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Articles);
        Assert.AreEqual(1, result.Articles.Count);
    }
}

