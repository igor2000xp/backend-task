using System.ComponentModel.DataAnnotations;
using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Domain.Tests.Entities;

[TestClass]
public class BlogEntityTests
{
    private const string TestUserId = "test-user-id-123";

    [TestMethod]
    public void BlogEntity_NameValidation_ShouldFailWhenTooShort()
    {
        // Arrange
        var blog = new BlogEntity { Name = "Short", UserId = TestUserId };
        var context = new ValidationContext(blog);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(blog, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Name")));
    }
    
    [TestMethod]
    public void BlogEntity_NameValidation_ShouldPassWhenValid()
    {
        // Arrange
        var blog = new BlogEntity { Name = "Valid Blog Name", UserId = TestUserId };
        var context = new ValidationContext(blog);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(blog, context, results, true);
        
        // Assert
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void BlogEntity_NameValidation_ShouldFailWhenTooLong()
    {
        // Arrange
        var blog = new BlogEntity { Name = "This is a very long blog name that exceeds the fifty character limit", UserId = TestUserId };
        var context = new ValidationContext(blog);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(blog, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Name")));
    }

    [TestMethod]
    public void BlogEntity_NameValidation_ShouldFailWhenEmpty()
    {
        // Arrange
        var blog = new BlogEntity { Name = "", UserId = TestUserId };
        var context = new ValidationContext(blog);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(blog, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void BlogEntity_ArticlesCollection_ShouldBeInitialized()
    {
        // Arrange & Act
        var blog = new BlogEntity();
        
        // Assert
        Assert.IsNotNull(blog.Articles);
        Assert.AreEqual(0, blog.Articles.Count);
    }

    [TestMethod]
    public void BlogEntity_UserIdValidation_ShouldFailWhenEmpty()
    {
        // Arrange
        var blog = new BlogEntity { Name = "Valid Blog Name", UserId = "" };
        var context = new ValidationContext(blog);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(blog, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("UserId")));
    }
}
