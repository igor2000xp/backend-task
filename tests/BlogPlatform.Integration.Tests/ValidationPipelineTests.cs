using System.ComponentModel.DataAnnotations;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Integration.Tests;

[TestClass]
public class ValidationPipelineTests
{
    [TestMethod]
    public async Task DbContext_SaveChanges_ShouldValidateBeforeDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase($"ValidationTests_{Guid.NewGuid()}")
            .Options;
        
        using var context = new BlogsContext(options);
        var invalidBlog = new BlogEntity
        {
            Name = "Too long name exceeding fifty character maximum limit here",
            IsActive = true
        };
        
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
            Assert.IsTrue(ex.Message.Contains("Name") || ex.Message.Contains("StringLength"),
                $"Unexpected validation message: {ex.Message}");
        }
    }
    
    [TestMethod]
    public async Task DbContext_SaveChanges_ShouldValidateMinimumLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase($"ValidationTests_{Guid.NewGuid()}")
            .Options;
        
        using var context = new BlogsContext(options);
        var invalidBlog = new BlogEntity
        {
            Name = "Short",
            IsActive = true
        };
        
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
    public async Task DbContext_SaveChanges_ShouldValidateRequiredFields()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase($"ValidationTests_{Guid.NewGuid()}")
            .Options;
        
        using var context = new BlogsContext(options);
        var invalidBlog = new BlogEntity
        {
            Name = string.Empty, // Required field is empty
            IsActive = true
        };
        
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
    public async Task DbContext_SaveChanges_ValidData_ShouldSucceed()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase($"ValidationTests_{Guid.NewGuid()}")
            .Options;
        
        using var context = new BlogsContext(options);
        var validBlog = new BlogEntity
        {
            Name = "Valid Blog Name",
            IsActive = true
        };
        
        // Act
        context.Blogs.Add(validBlog);
        await context.SaveChangesAsync();
        
        // Assert
        Assert.IsTrue(validBlog.BlogId > 0, "Blog should have been assigned an ID");
    }
    
    [TestMethod]
    public async Task DbContext_SaveChanges_PostValidation_ShouldEnforceContentLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase($"ValidationTests_{Guid.NewGuid()}")
            .Options;
        
        using var context = new BlogsContext(options);
        
        // Create a valid blog first
        var blog = new BlogEntity { Name = "Valid Blog Name", IsActive = true };
        context.Blogs.Add(blog);
        await context.SaveChangesAsync();
        
        // Create post with content too long
        var longContent = new string('a', 1001); // 1001 characters
        var invalidPost = new PostEntity
        {
            Name = "Valid Post Name",
            Content = longContent,
            ParentId = blog.BlogId,
            Created = DateTime.UtcNow
        };
        
        // Act & Assert
        context.Posts.Add(invalidPost);
        try
        {
            await context.SaveChangesAsync();
            Assert.Fail("Expected ValidationException was not thrown");
        }
        catch (ValidationException ex)
        {
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message.Contains("Content") || ex.Message.Contains("StringLength"),
                $"Unexpected validation message: {ex.Message}");
        }
    }
    
    [TestMethod]
    public async Task DbContext_Update_ShouldAlsoValidate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase($"ValidationTests_{Guid.NewGuid()}")
            .Options;
        
        using var context = new BlogsContext(options);
        
        // Create a valid blog
        var blog = new BlogEntity { Name = "Valid Blog Name", IsActive = true };
        context.Blogs.Add(blog);
        await context.SaveChangesAsync();
        
        // Act - Try to update with invalid data
        blog.Name = "Bad"; // Too short
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
}

