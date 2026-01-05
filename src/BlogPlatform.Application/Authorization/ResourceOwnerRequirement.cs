using Microsoft.AspNetCore.Authorization;

namespace BlogPlatform.Application.Authorization;

/// <summary>
/// Authorization requirement for resource ownership.
/// A user must either own the resource or be an Admin to satisfy this requirement.
/// </summary>
public class ResourceOwnerRequirement : IAuthorizationRequirement
{
}

