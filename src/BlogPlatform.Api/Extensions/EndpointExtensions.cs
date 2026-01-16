using BlogPlatform.Api.Abstractions;

namespace BlogPlatform.Api.Extensions;

/// <summary>
/// Extension methods for automatic endpoint discovery and registration
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Scans the assembly for IEndpoint implementations and registers them automatically
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && !t.IsInterface && !t.IsAbstract);

        foreach (var endpointType in endpointTypes)
        {
            var endpoint = (IEndpoint?)Activator.CreateInstance(endpointType);
            endpoint?.MapEndpoint(app);
        }

        return app;
    }
}
