using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Tests.Configurations;

[TestClass]
public class PostConfigurationTests
{
    [TestMethod]
    public async Task PostConfiguration_DataStoredAndRetrieved_Successfully()
    {
        // Arrange - Verify post configuration works correctly by storing and retrieving data
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
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
        
        // Act
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        // Assert - Configuration is working if data can be stored and retrieved
        var storedPost = await context.Posts.FindAsync(post.PostId);
        Assert.IsNotNull(storedPost, "Post should be stored and retrieved successfully");
        Assert.AreEqual("Test Post Name", storedPost.Name);
    }

    [TestMethod]
    public async Task PostConfiguration_ForeignKey_ShouldLinkToBlog()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        context.Blogs.Add(blog);
        await context.SaveChangesAsync();
        
        // Act
        var post = new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content for the post",
            ParentId = blog.BlogId,
            Created = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        // Assert
        var savedPost = await context.Posts
            .Include(p => p.Blog)
            .FirstOrDefaultAsync(p => p.PostId == post.PostId);
        
        Assert.IsNotNull(savedPost);
        Assert.IsNotNull(savedPost.Blog);
        Assert.AreEqual(blog.BlogId, savedPost.ParentId);
        Assert.AreEqual(blog.Name, savedPost.Blog.Name);
    }

    [TestMethod]
    public async Task PostConfiguration_PrimaryKey_ShouldBeConfigured()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
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
        
        // Act
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        // Assert
        Assert.IsTrue(post.PostId > 0, "Primary key should be auto-generated");
    }

    [TestMethod]
    public async Task PostConfiguration_UpdatedProperty_ShouldBeNullable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        
        var blog = new BlogEntity { Name = "Test Blog Name", IsActive = true };
        context.Blogs.Add(blog);
        await context.SaveChangesAsync();
        
        var post = new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content",
            ParentId = blog.BlogId,
            Created = DateTime.UtcNow,
            Updated = null // Explicitly null
        };
        
        // Act
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        // Assert
        var savedPost = await context.Posts.FindAsync(post.PostId);
        Assert.IsNotNull(savedPost);
        Assert.IsNull(savedPost.Updated);
    }
}

