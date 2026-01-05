namespace BlogPlatform.Application.Authorization;

/// <summary>
/// Interface for resources that have an owner (UserId)
/// </summary>
public interface IResourceWithOwner
{
    /// <summary>
    /// The ID of the user who owns this resource
    /// </summary>
    string UserId { get; }
}

