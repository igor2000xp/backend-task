using System.ComponentModel.DataAnnotations;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Tests.Configurations;

[TestClass]
public class BlogConfigurationTests
{
    private DbContextOptions<BlogsContext> GetInMemoryOptions()
    {
        return new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }
    
    [TestMethod]
    public async Task BlogConfiguration_IsActiveConversion_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange
        var options = GetInMemoryOptions();
        using var context = new BlogsContext(options);
        
        var blogActive = new BlogEntity { Name = "Active Blog Name", IsActive = true };
        var blogInactive = new BlogEntity { Name = "Inactive Blog", IsActive = false };
        
        // Act
        context.Blogs.Add(blogActive);
        context.Blogs.Add(blogInactive);
        await context.SaveChangesAsync();
        
        // Clear tracking to force read from database
        context.ChangeTracker.Clear();
        
        // Assert - Verify the values are stored and retrieved correctly
        var storedActive = await context.Blogs.FirstOrDefaultAsync(b => b.BlogId == blogActive.BlogId);
        var storedInactive = await context.Blogs.FirstOrDefaultAsync(b => b.BlogId == blogInactive.BlogId);
        
        Assert.IsNotNull(storedActive);
        Assert.IsTrue(storedActive.IsActive, "Active blog should be true");
        
        Assert.IsNotNull(storedInactive);
        Assert.IsFalse(storedInactive.IsActive, "Inactive blog should be false");
    }
    
    [TestMethod]
    public void BlogsContext_SaveChanges_ShouldValidateBeforeSaving()
    {
        // Arrange
        var options = GetInMemoryOptions();
        using var context = new BlogsContext(options);
        var invalidBlog = new BlogEntity { Name = "Bad" }; // Too short
        
        // Act & Assert
        context.Blogs.Add(invalidBlog);
        try
        {
            context.SaveChanges();
            Assert.Fail("Expected ValidationException was not thrown");
        }
        catch (ValidationException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    [TestMethod]
    public async Task BlogsContext_SaveChangesAsync_ShouldValidateBeforeSaving()
    {
        // Arrange
        var options = GetInMemoryOptions();
        using var context = new BlogsContext(options);
        var invalidBlog = new BlogEntity { Name = "Too long name exceeding the maximum fifty character limit here" };
        
        // Act & Assert
        context.Blogs.Add(invalidBlog);
        try
        {
            await context.SaveChangesAsync();
            Assert.Fail("Expected ValidationException was not thrown");
        }
        catch (ValidationException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    [TestMethod]
    public async Task BlogConfiguration_PrimaryKey_ShouldBeConfigured()
    {
        // Arrange
        var options = GetInMemoryOptions();
        using var context = new BlogsContext(options);
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        
        // Act
        context.Blogs.Add(blog);
        await context.SaveChangesAsync();
        
        // Assert
        Assert.IsTrue(blog.BlogId > 0, "Primary key should be auto-generated");
    }

    [TestMethod]
    public async Task BlogConfiguration_CascadeDelete_ShouldDeleteArticles()
    {
        // Arrange
        var options = GetInMemoryOptions();
        using var context = new BlogsContext(options);
        
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        context.Blogs.Add(blog);
        await context.SaveChangesAsync();
        
        var post = new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content",
            ParentId = blog.BlogId,
            Created = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        // Act - Delete the blog
        context.Blogs.Remove(blog);
        await context.SaveChangesAsync();
        
        // Assert - Post should be cascade deleted
        var remainingPosts = await context.Posts.CountAsync();
        Assert.AreEqual(0, remainingPosts, "Posts should be cascade deleted with blog");
    }
}

