namespace BlogPlatform.Api.Abstractions;

/// <summary>
/// Interface for defining minimal API endpoints using the REPR pattern
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the application's route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
