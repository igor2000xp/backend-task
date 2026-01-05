using System.ComponentModel.DataAnnotations;
using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Domain.Tests.Entities;

[TestClass]
public class PostEntityTests
{
    private const string TestUserId = "test-user-id-123";

    [TestMethod]
    public void PostEntity_NameValidation_ShouldFailWhenTooShort()
    {
        // Arrange
        var post = new PostEntity { Name = "Short", Content = "Valid content here", UserId = TestUserId };
        var context = new ValidationContext(post);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(post, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Name")));
    }
    
    [TestMethod]
    public void PostEntity_NameValidation_ShouldPassWhenValid()
    {
        // Arrange
        var post = new PostEntity { Name = "Valid Post Name", Content = "Valid content here", UserId = TestUserId };
        var context = new ValidationContext(post);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(post, context, results, true);
        
        // Assert
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void PostEntity_ContentValidation_ShouldFailWhenEmpty()
    {
        // Arrange
        var post = new PostEntity { Name = "Valid Post Name", Content = "", UserId = TestUserId };
        var context = new ValidationContext(post);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(post, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Content")));
    }

    [TestMethod]
    public void PostEntity_ContentValidation_ShouldFailWhenTooLong()
    {
        // Arrange
        var longContent = new string('a', 1001); // 1001 characters
        var post = new PostEntity { Name = "Valid Post Name", Content = longContent, UserId = TestUserId };
        var context = new ValidationContext(post);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(post, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Content")));
    }

    [TestMethod]
    public void PostEntity_UpdatedProperty_ShouldBeNullable()
    {
        // Arrange & Act
        var post = new PostEntity
        {
            Name = "Valid Post Name",
            Content = "Valid content",
            UserId = TestUserId,
            Created = DateTime.UtcNow
        };
        
        // Assert
        Assert.IsNull(post.Updated);
    }

    [TestMethod]
    public void PostEntity_UserIdValidation_ShouldFailWhenEmpty()
    {
        // Arrange
        var post = new PostEntity { Name = "Valid Post Name", Content = "Valid content", UserId = "" };
        var context = new ValidationContext(post);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(post, context, results, true);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains("UserId")));
    }
}
