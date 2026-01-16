# Technical Context

## Technology Stack

### Core Framework
- **.NET 10.0**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **C#**: Primary programming language

### Authentication & Authorization
- **ASP.NET Core Identity**: User management and authentication
- **JWT Bearer Authentication**: Token-based authentication
- **System.IdentityModel.Tokens.Jwt**: JWT token handling

### Data Access
- **Entity Framework Core 10.0**: ORM for database operations
- **SQLite**: Development database (file-based)
- **SQL Server 2022**: Production database (Docker container)

### Testing
- **MSTest**: Unit and integration testing framework
- **Moq**: Mocking framework for unit tests
- **Microsoft.AspNetCore.Mvc.Testing**: Integration test support

### API Documentation
- **OpenAPI**: API specification
- **Scalar**: Interactive API documentation UI

### Security
- **Rate Limiting**: Built-in ASP.NET Core rate limiting
- **Security Headers**: X-Frame-Options, CSP, etc.

## Development Setup

### Prerequisites
- .NET 10.0 SDK
- Docker (for SQL Server in production)
- IDE: Visual Studio, VS Code, or Rider

### Project Structure
```
BlogPlatform/
├── src/
│   ├── BlogPlatform.Domain/          # Domain entities and interfaces
│   ├── BlogPlatform.Application/     # Business logic and services
│   ├── BlogPlatform.Infrastructure/  # Data access and external services
│   └── BlogPlatform.Api/             # Web API and controllers
└── tests/                            # Test projects
```

### Dependency Injection

Service registration is done through extension methods:

**Application Layer**:
- `AddApplicationServices()`: Registers application services, auth services, and authorization handlers

**Infrastructure Layer**:
- `AddInfrastructureServices()`: Registers database context, Identity, and repositories

**Program.cs**:
- Uses extension methods for clean service registration
- Organized into clear sections for maintainability

### Database Configuration

- **Development**: SQLite (file-based, no setup required)
- **Production**: SQL Server (via Docker Compose)
- **Testing**: In-memory database (via CustomWebApplicationFactory)

### Configuration Files

- `appsettings.json`: Development configuration
- `appsettings.Production.json`: Production configuration (uses environment variables)
- `appsettings.Development.json`: Development-specific overrides

### Environment Variables (Production)

```bash
JWT_SECRET_KEY=your-secret-key
JWT_ISSUER=BlogPlatformApi
JWT_AUDIENCE=BlogPlatformClient
CONNECTION_STRING=Server=...;Database=BlogPlatform;...
```

## Build & Run

### Development
```bash
cd src/BlogPlatform.Api
dotnet run
```

### Testing
```bash
dotnet test
```

### Database Migrations
```bash
cd src/BlogPlatform.Api
dotnet ef database update --project ../BlogPlatform.Infrastructure
```

## Code Organization Principles

1. **Layer Separation**: Clear boundaries between Domain, Application, Infrastructure, and Presentation
2. **Dependency Inversion**: Depend on abstractions (interfaces), not implementations
3. **Extension Methods**: Use extension methods for DI registration
4. **Clear Sections**: Program.cs organized with section comments
5. **Testability**: All layers designed for easy testing
