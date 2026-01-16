# System Patterns

## Architecture Patterns

### Clean Architecture Layers

The solution follows Clean Architecture with four distinct layers:

1. **Domain Layer** (`BlogPlatform.Domain`)
   - Entities (BlogEntity, PostEntity, ApplicationUser, CompromisedRefreshToken)
   - Interfaces (IBlogRepository, IPostRepository, ICompromisedRefreshTokenRepository)
   - No dependencies on other layers

2. **Application Layer** (`BlogPlatform.Application`)
   - Services (BlogService, PostService, AuthService, TokenService)
   - DTOs (Data Transfer Objects)
   - Authorization handlers
   - Configuration classes (JwtSettings)
   - Dependency injection extension method (`AddApplicationServices`)
   - Depends only on Domain layer

3. **Infrastructure Layer** (`BlogPlatform.Infrastructure`)
   - Entity Framework Core DbContext (BlogsContext)
   - Repository implementations
   - Entity configurations
   - ASP.NET Core Identity integration
   - Dependency injection extension method (`AddInfrastructureServices`)
   - Depends on Domain and Application layers

4. **Presentation Layer** (`BlogPlatform.Api`)
   - Controllers (AuthController, BlogsController, PostsController)
   - Program.cs orchestration
   - Middleware configuration
   - OpenAPI/Swagger configuration
   - Depends on Application and Infrastructure layers

## Dependency Injection Pattern

### Extension Methods for DI

Service registration is organized using extension methods for better maintainability:

**Application Layer DI** (`BlogPlatform.Application/DependencyInjection.cs`):
```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Application services
    services.AddScoped<IBlogService, BlogService>();
    services.AddScoped<IPostService, PostService>();
    
    // Authentication services
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IEmailService, EmailService>();
    
    // Authorization handlers
    services.AddScoped<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();
    
    return services;
}
```

**Infrastructure Layer DI** (`BlogPlatform.Infrastructure/DependencyInjection.cs`):
```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services, 
    IConfiguration configuration, 
    IHostEnvironment environment)
{
    // Database configuration
    // Identity configuration
    // Repository registration
    
    return services;
}
```

### Program.cs Organization

Program.cs is organized into clear sections:
1. Configuration
2. Layer-Based Dependency Injection
3. API Specific Configuration
4. Database Initialization & Seeding
5. Middleware Pipeline

## Repository Pattern

- Repository interfaces defined in Domain layer
- Repository implementations in Infrastructure layer
- Services depend on interfaces, not implementations
- Enables easy testing and swapping implementations

## Service Pattern

- Services contain business logic
- Services depend on repository interfaces
- Services return DTOs, not entities
- Clear separation between data access and business logic

## Authorization Pattern

### Resource Owner Authorization

- Custom authorization handler (`ResourceOwnerAuthorizationHandler`)
- Checks if user is Admin OR resource owner
- Applied via `[Authorize(Policy = "ResourceOwner")]` attribute
- Services validate ownership before operations

## Authentication Pattern

### Dual-Token System

- Short-lived access tokens (15 minutes)
- Long-lived refresh tokens (7 days)
- Refresh token rotation on each refresh
- Compromised token blacklist for security

## Testing Patterns

- Unit tests for services and repositories
- Integration tests for end-to-end flows
- CustomWebApplicationFactory for integration test setup
- Moq for mocking dependencies
- MSTest framework
