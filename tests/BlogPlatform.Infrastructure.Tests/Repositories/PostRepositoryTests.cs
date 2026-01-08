using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Tests.Repositories;

[TestClass]
public class PostRepositoryTests
{
    [TestMethod]
    public async Task CreateAsync_ValidPost_ShouldReturnPostWithId()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var blogRepo = new BlogRepository(context);
        var postRepo = new PostRepository(context);
        
        var blog = await blogRepo.CreateAsync(new BlogEntity { Name = "Test Blog Name", IsActive = true });
        var post = new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content for the post",
            ParentId = blog.BlogId
        };
        
        // Act
        var result = await postRepo.CreateAsync(post);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.PostId > 0);
        Assert.AreEqual("Test Post Name", result.Name);
        Assert.IsTrue(result.Created != default(DateTime));
    }

    [TestMethod]
    public async Task GetByIdAsync_ExistingPost_ShouldReturnPost()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var blogRepo = new BlogRepository(context);
        var postRepo = new PostRepository(context);
        
        var blog = await blogRepo.CreateAsync(new BlogEntity { Name = "Test Blog Name", IsActive = true });
        var post = await postRepo.CreateAsync(new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content",
            ParentId = blog.BlogId
        });
        
        // Act
        var result = await postRepo.GetByIdAsync(post.PostId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(post.PostId, result.PostId);
        Assert.AreEqual("Test Post Name", result.Name);
        Assert.IsNotNull(result.Blog);
    }

    [TestMethod]
    public async Task GetByIdAsync_NonExistingPost_ShouldReturnNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var postRepo = new PostRepository(context);
        
        // Act
        var result = await postRepo.GetByIdAsync(999);
        
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByBlogIdAsync_MultiplePosts_ShouldReturnOnlyBlogPosts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var blogRepo = new BlogRepository(context);
        var postRepo = new PostRepository(context);
        
        var blog1 = await blogRepo.CreateAsync(new BlogEntity { Name = "First Blog Name", IsActive = true });
        var blog2 = await blogRepo.CreateAsync(new BlogEntity { Name = "Second Blog Name", IsActive = true });
        
        await postRepo.CreateAsync(new PostEntity { Name = "Post 1 Blog 1", Content = "Content", ParentId = blog1.BlogId });
        await postRepo.CreateAsync(new PostEntity { Name = "Post 2 Blog 1", Content = "Content", ParentId = blog1.BlogId });
        await postRepo.CreateAsync(new PostEntity { Name = "Post 1 Blog 2", Content = "Content", ParentId = blog2.BlogId });
        
        // Act
        var result = await postRepo.GetByBlogIdAsync(blog1.BlogId);
        
        // Assert
        var posts = result.ToList();
        Assert.AreEqual(2, posts.Count);
        Assert.IsTrue(posts.All(p => p.ParentId == blog1.BlogId));
    }

    [TestMethod]
    public async Task GetAllAsync_MultiplePosts_ShouldReturnAll()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var blogRepo = new BlogRepository(context);
        var postRepo = new PostRepository(context);
        
        var blog = await blogRepo.CreateAsync(new BlogEntity { Name = "Test Blog Name", IsActive = true });
        
        await postRepo.CreateAsync(new PostEntity { Name = "First Post Name", Content = "Content", ParentId = blog.BlogId });
        await postRepo.CreateAsync(new PostEntity { Name = "Second Post Name", Content = "Content", ParentId = blog.BlogId });
        
        // Act
        var result = await postRepo.GetAllAsync();
        
        // Assert
        var posts = result.ToList();
        Assert.AreEqual(2, posts.Count);
    }

    [TestMethod]
    public async Task UpdateAsync_ExistingPost_ShouldUpdatePropertiesAndSetUpdatedTime()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var blogRepo = new BlogRepository(context);
        var postRepo = new PostRepository(context);
        
        var blog = await blogRepo.CreateAsync(new BlogEntity { Name = "Test Blog Name", IsActive = true });
        var post = await postRepo.CreateAsync(new PostEntity 
        { 
            Name = "Original Name", 
            Content = "Original content",
            ParentId = blog.BlogId
        });
        
        // Act
        post.Name = "Updated Post Name";
        post.Content = "Updated content";
        await postRepo.UpdateAsync(post);
        
        // Assert
        var updated = await postRepo.GetByIdAsync(post.PostId);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Post Name", updated.Name);
        Assert.AreEqual("Updated content", updated.Content);
        Assert.IsNotNull(updated.Updated);
    }

    [TestMethod]
    public async Task DeleteAsync_ExistingPost_ShouldRemovePost()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new BlogsContext(options);
        var blogRepo = new BlogRepository(context);
        var postRepo = new PostRepository(context);
        
        var blog = await blogRepo.CreateAsync(new BlogEntity { Name = "Test Blog Name", IsActive = true });
        var post = await postRepo.CreateAsync(new PostEntity 
        { 
            Name = "Test Post Name", 
            Content = "Test content",
            ParentId = blog.BlogId
        });
        
        // Act
        await postRepo.DeleteAsync(post.PostId);
        
        // Assert
        var deleted = await postRepo.GetByIdAsync(post.PostId);
        Assert.IsNull(deleted);
    }
}

