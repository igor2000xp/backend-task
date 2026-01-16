# Blog Platform - ASP.NET Core SaaS Application

## Project Overview

A production-ready SaaS blogging platform built with ASP.NET Core and Entity Framework Core, implementing Clean Architecture principles. The application provides a RESTful API for managing blogs and posts with comprehensive validation, proper database mapping, **JWT-based authentication**, **role-based authorization**, and full test coverage.

## 🔐 Authentication & Authorization

### Overview

The platform implements a secure, industry-standard JWT-based authentication system with:

- **Dual-token system**: Short-lived access tokens (15 min) + long-lived refresh tokens (7 days)
- **Role-based access control (RBAC)**: Admin and User roles with granular permissions
- **Resource ownership**: Users can only modify their own blogs/posts; Admins can modify all content
- **Compromised token blacklist**: Protection against token reuse after logout
- **Rate limiting**: Protection against brute-force attacks

### Authentication Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    JWT Authentication Flow                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Register/Login                                              │
│     ┌──────┐       POST /api/auth/register or /login           │
│     │Client│ ─────────────────────────────────────────────────►│
│     └──────┘                                                    │
│         │                                                        │
│         │         Response: AccessToken + RefreshToken           │
│         │◄────────────────────────────────────────────────────  │
│                                                                  │
│  2. Access Protected Resources                                   │
│     ┌──────┐       Authorization: Bearer <AccessToken>          │
│     │Client│ ─────────────────────────────────────────────────►│
│     └──────┘                                                    │
│         │                                                        │
│         │         Response: Protected data                       │
│         │◄────────────────────────────────────────────────────  │
│                                                                  │
│  3. Refresh Token (when access token expires)                   │
│     ┌──────┐       POST /api/auth/refresh                       │
│     │Client│       Body: { accessToken, refreshToken }          │
│     └──────┘ ─────────────────────────────────────────────────►│
│         │                                                        │
│         │         Response: New AccessToken + New RefreshToken   │
│         │◄────────────────────────────────────────────────────  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Default Users (Seeded)

| Email | Password | Role | Permissions |
|-------|----------|------|-------------|
| `admin@blogplatform.com` | `Admin@123456` | Admin | Full access to all resources |
| `user@blogplatform.com` | `User@123456` | User | Own resources only |

### JWT Configuration

Configure in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "BlogPlatformApi",
    "Audience": "BlogPlatformClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

> ⚠️ **Security Note**: In production, use environment variables for sensitive values.

### Production Environment Variables

```bash
JWT_SECRET_KEY=your-strong-secret-key-at-least-32-characters
JWT_ISSUER=BlogPlatformApi
JWT_AUDIENCE=BlogPlatformClient
CONNECTION_STRING=Server=...;Database=BlogPlatform;...
```

## Architecture

### Clean Architecture Layers

The solution follows Clean Architecture with clear separation of concerns across four distinct layers:

```
┌─────────────────────────────────────────┐
│   Presentation Layer (API)              │
│   - Controllers                         │
│   - Authentication/Authorization        │
│   - Rate Limiting                       │
│   - OpenAPI Documentation               │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Application Layer                     │
│   - Services                            │
│   - DTOs                                │
│   - Authentication Services             │
│   - Authorization Handlers              │
│   - DependencyInjection.cs              │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Infrastructure Layer                  │
│   - Entity Framework Core               │
│   - ASP.NET Core Identity               │
│   - Repositories                        │
│   - Database Configurations             │
│   - DependencyInjection.cs              │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Domain Layer                          │
│   - Entities (incl. ApplicationUser)    │
│   - Interfaces                          │
│   - Validation Rules                    │
└─────────────────────────────────────────┘
```

### Code Organization & Dependency Injection

The project uses **extension methods** for dependency injection to maintain clean separation of concerns and improve maintainability:

#### Application Layer DI (`BlogPlatform.Application/DependencyInjection.cs`)
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

#### Infrastructure Layer DI (`BlogPlatform.Infrastructure/DependencyInjection.cs`)
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

#### Program.cs Organization

The `Program.cs` file is organized into clear sections for better readability and maintainability:

1. **Configuration**: JWT settings and configuration setup
2. **Layer-Based Dependency Injection**: Calls to extension methods (`AddApplicationServices`, `AddInfrastructureServices`)
3. **API Specific Configuration**: Background services, authentication, authorization, rate limiting
4. **Database Initialization & Seeding**: Migration and seed data logic
5. **Middleware Pipeline**: Security headers, rate limiting, authentication, authorization

**Benefits:**
- ✅ Improved code maintainability
- ✅ Better adherence to Clean Architecture principles
- ✅ Clear layer boundaries
- ✅ Easier to test and modify service registrations
- ✅ Reduced complexity in Program.cs

## Technology Stack

### Core Technologies
- **.NET 10.0**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **ASP.NET Core Identity**: User management and authentication
- **Entity Framework Core 10.0**: ORM for database operations
- **JWT Authentication**: Industry-standard token-based auth
- **MSTest**: Unit and integration testing framework
- **Moq**: Mocking framework for unit tests

### Security
- **Rate Limiting**: Built-in ASP.NET Core rate limiting
- **Security Headers**: X-Frame-Options, X-Content-Type-Options, etc.
- **Password Hashing**: BCrypt via ASP.NET Core Identity
- **Token Blacklisting**: Compromised refresh token tracking

### Database Providers
- **SQLite**: Development and testing (file-based)
- **SQL Server 2022**: Production deployment (Docker container)

## API Endpoints

### Authentication API

| Method | Endpoint | Auth | Description | Status Codes |
|--------|----------|------|-------------|--------------|
| POST | `/api/auth/register` | 🔓 | Register new user | 200, 400 |
| POST | `/api/auth/login` | 🔓 | Login with credentials | 200, 401 |
| POST | `/api/auth/refresh` | 🔓 | Refresh access token | 200, 401 |
| POST | `/api/auth/logout` | 🔒 | Logout (blacklist token) | 200, 401 |
| POST | `/api/auth/revoke-all/{userId}` | 🔒 Admin | Revoke all user tokens | 200, 401, 403 |
| GET | `/api/auth/me` | 🔒 | Get current user info | 200, 401 |

### Blogs API

| Method | Endpoint | Auth | Description | Status Codes |
|--------|----------|------|-------------|--------------|
| GET | `/api/blogs` | 🔓 | Get all blogs | 200 |
| GET | `/api/blogs/{id}` | 🔓 | Get blog by ID | 200, 404 |
| POST | `/api/blogs` | 🔒 | Create new blog | 201, 400, 401 |
| PUT | `/api/blogs/{id}` | 🔒 Owner/Admin | Update blog | 204, 401, 403, 404 |
| DELETE | `/api/blogs/{id}` | 🔒 Owner/Admin | Delete blog | 204, 401, 403, 404 |

### Posts API

| Method | Endpoint | Auth | Description | Status Codes |
|--------|----------|------|-------------|--------------|
| GET | `/api/posts` | 🔓 | Get all posts | 200 |
| GET | `/api/posts/{id}` | 🔓 | Get post by ID | 200, 404 |
| GET | `/api/posts/blog/{blogId}` | 🔓 | Get posts by blog | 200 |
| POST | `/api/posts` | 🔒 | Create new post | 201, 400, 401 |
| PUT | `/api/posts/{id}` | 🔒 Owner/Admin | Update post | 204, 401, 403, 404 |
| DELETE | `/api/posts/{id}` | 🔒 Owner/Admin | Delete post | 204, 401, 403, 404 |

**Legend**: 🔓 Public | 🔒 Requires Authentication

### API Examples

#### Register User
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePass@123",
    "confirmPassword": "SecurePass@123",
    "fullName": "New User"
  }'
```

#### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@blogplatform.com",
    "password": "User@123456"
  }'
```

**Response:**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "abc123...",
  "userId": "user-guid",
  "email": "user@blogplatform.com",
  "roles": ["User"]
}
```

#### Create Blog (Authenticated)
```bash
curl -X POST https://localhost:5001/api/blogs \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <access_token>" \
  -d '{
    "name": "My Awesome Blog",
    "isActive": true
  }'
```

#### Refresh Token
```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "accessToken": "expired-access-token",
    "refreshToken": "valid-refresh-token"
  }'
```

## Security Features

### Rate Limiting

The API implements rate limiting to protect against abuse:

| Policy | Limit | Window | Applied To |
|--------|-------|--------|------------|
| `auth` | 10 requests | 1 minute | Authentication endpoints |
| `api` | 50 requests | 30 seconds | Blog/Post endpoints |
| `fixed` | 100 requests | 1 minute | Global fallback |

**Rate Limit Exceeded Response (429):**
```json
{
  "error": "Too many requests. Please try again later.",
  "retryAfterSeconds": 30
}
```

### Security Headers

All responses include security headers:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Content-Security-Policy: default-src 'self'`
- `Permissions-Policy: camera=(), microphone=(), geolocation=()`

### Password Requirements

Enforced by ASP.NET Core Identity:

- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character
- At least 4 unique characters

### Account Lockout

- 5 failed attempts triggers 15-minute lockout
- Protects against brute-force attacks

## Testing

### Test Coverage Summary

**Total: 126 tests passing**

| Layer | Test Count | Coverage Focus |
|-------|------------|----------------|
| Domain | 13 tests | Entity validation, Data Annotations, User ownership |
| Infrastructure | 24 tests | EF configurations, repositories, validation pipeline |
| Application | 45 tests | Service logic, authentication, token handling |
| Integration | 44 tests | End-to-end flows, auth flows, authorization rules |

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/BlogPlatform.Integration.Tests/

# Run with verbose output
dotnet test --verbosity normal
```

## Development

### Prerequisites

- .NET 10.0 SDK
- Docker (for SQL Server in production)

### Run Locally

```bash
# Navigate to API project
cd src/BlogPlatform.Api

# Run the application
dotnet run
```

### Apply Migrations

```bash
cd src/BlogPlatform.Api
dotnet ef database update
```

### Access API Documentation

- **Scalar UI** (recommended): `https://localhost:5001/scalar`
- **OpenAPI spec**: `https://localhost:5001/openapi/v1.json`

## Production Deployment

### Docker Compose

```bash
docker-compose up -d
```

### Environment Variables

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - JWT_SECRET_KEY=your-secret-key
  - JWT_ISSUER=BlogPlatformApi
  - JWT_AUDIENCE=BlogPlatformClient
  - CONNECTION_STRING=Server=sql-server;Database=BlogPlatform;...
```

## Project Structure

```
BlogPlatform/
├── src/
│   ├── BlogPlatform.Domain/
│   │   └── Entities/
│   │       ├── ApplicationUser.cs        # Custom Identity user
│   │       ├── BlogEntity.cs
│   │       ├── PostEntity.cs
│   │       └── CompromisedRefreshToken.cs
│   │
│   ├── BlogPlatform.Application/
│   │   ├── Configuration/
│   │   │   └── JwtSettings.cs
│   │   ├── DependencyInjection.cs      # DI extension method
│   │   ├── DTOs/Auth/
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   ├── RefreshTokenRequest.cs
│   │   │   └── AuthenticationResult.cs
│   │   ├── Authorization/
│   │   │   └── ResourceOwnerAuthorizationHandler.cs
│   │   └── Services/
│   │       ├── AuthService.cs
│   │       ├── TokenService.cs
│   │       └── EmailService.cs (stub)
│   │
│   ├── BlogPlatform.Infrastructure/
│   │   ├── DependencyInjection.cs      # DI extension method
│   │   ├── Data/
│   │   │   ├── BlogsContext.cs          # IdentityDbContext
│   │   │   └── SeedData.cs
│   │   └── Configurations/
│   │       └── CompromisedRefreshTokenConfiguration.cs
│   │
│   └── BlogPlatform.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── BlogsController.cs
│       │   └── PostsController.cs
│       ├── BackgroundServices/
│       │   └── TokenCleanupService.cs
│       └── Program.cs
│
└── tests/
    ├── BlogPlatform.Domain.Tests/
    ├── BlogPlatform.Application.Tests/
    ├── BlogPlatform.Infrastructure.Tests/
    └── BlogPlatform.Integration.Tests/
```

## Future Enhancements

- [ ] Email confirmation workflow
- [ ] Password reset via email
- [ ] Two-factor authentication (2FA)
- [ ] OAuth2/OpenID Connect integration
- [ ] API key authentication for service-to-service
- [ ] Audit logging for security events
- [ ] Redis-based token blacklist for distributed systems

## License

This project is part of a technical implementation demonstration.
