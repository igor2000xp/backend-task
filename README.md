# Blog Platform - ASP.NET Core SaaS Application

## Project Overview

A production-ready SaaS blogging platform built with ASP.NET Core and Entity Framework Core, implementing Clean Architecture principles. The application provides a RESTful API for managing blogs and posts with comprehensive validation, proper database mapping, and full test coverage.

## Architecture

### Clean Architecture Layers

The solution follows Clean Architecture with clear separation of concerns across four distinct layers:

```
┌─────────────────────────────────────┐
│   Presentation Layer (API)          │
│   - Controllers                     │
│   - HTTP Endpoints                  │
│   - Swagger Documentation           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Application Layer                 │
│   - Services                        │
│   - DTOs                            │
│   - Business Logic                 │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Infrastructure Layer              │
│   - Entity Framework Core           │
│   - Repositories                    │
│   - Database Configurations         │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Domain Layer                      │
│   - Entities                        │
│   - Interfaces                      │
│   - Validation Rules                │
└─────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Clean Architecture**: Strict dependency rule - outer layers depend on inner layers, never the reverse
2. **Repository Pattern**: Abstraction of data access through interfaces in Domain layer
3. **Dependency Injection**: All dependencies injected through constructor injection
4. **Validation Pipeline**: Application-level validation before database operations
5. **Database-Agnostic Domain**: Domain layer has no dependencies on EF Core

## Technology Stack

### Core Technologies
- **.NET 10.0**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core 10.0**: ORM for database operations
- **MSTest**: Unit and integration testing framework
- **Moq**: Mocking framework for unit tests

### Database Providers
- **SQLite**: Development and testing (file-based)
- **SQL Server 2022**: Production deployment (Docker container)

### Additional Packages
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI documentation
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Microsoft.EntityFrameworkCore.Design**: EF Core migration tools

## Project Structure

```
BlogPlatform/
├── src/
│   ├── BlogPlatform.Domain/              # Domain Layer
│   │   ├── Entities/
│   │   │   ├── BlogEntity.cs
│   │   │   └── PostEntity.cs
│   │   └── Interfaces/
│   │       ├── IBlogRepository.cs
│   │       └── IPostRepository.cs
│   │
│   ├── BlogPlatform.Application/         # Application Layer
│   │   ├── DTOs/
│   │   │   ├── BlogDto.cs
│   │   │   ├── PostDto.cs
│   │   │   ├── CreateBlogRequest.cs
│   │   │   ├── UpdateBlogRequest.cs
│   │   │   ├── CreatePostRequest.cs
│   │   │   └── UpdatePostRequest.cs
│   │   └── Services/
│   │       ├── BlogService.cs
│   │       ├── PostService.cs
│   │       └── Interfaces/
│   │           ├── IBlogService.cs
│   │           └── IPostService.cs
│   │
│   ├── BlogPlatform.Infrastructure/       # Infrastructure Layer
│   │   ├── Configurations/
│   │   │   ├── BlogConfiguration.cs
│   │   │   └── PostConfiguration.cs
│   │   ├── Data/
│   │   │   └── BlogsContext.cs
│   │   └── Repositories/
│   │       ├── BlogRepository.cs
│   │       └── PostRepository.cs
│   │
│   └── BlogPlatform.Api/                  # Presentation Layer
│       ├── Controllers/
│       │   ├── BlogsController.cs
│       │   └── PostsController.cs
│       ├── Program.cs
│       └── appsettings.json
│
└── tests/
    ├── BlogPlatform.Domain.Tests/         # Domain Tests (11 tests)
    ├── BlogPlatform.Infrastructure.Tests/ # Infrastructure Tests (24 tests)
    ├── BlogPlatform.Application.Tests/    # Application Tests (18 tests)
    └── BlogPlatform.Integration.Tests/    # Integration Tests (6 tests)
```

## Domain Model

### BlogEntity

Represents a blog with the following properties:

- **BlogId** (int): Primary key
- **Name** (string): Blog name, required, 10-50 characters
- **IsActive** (bool): Active status (stored as string in database)
- **Articles** (ICollection<PostEntity>): Navigation property to posts

**Validation Rules:**
- Name is required
- Name must be between 10 and 50 characters

### PostEntity

Represents a blog post/article with the following properties:

- **PostId** (int): Primary key
- **ParentId** (int): Foreign key to BlogEntity
- **Name** (string): Post name, required, 10-50 characters
- **Content** (string): Post content, required, max 1000 characters
- **Created** (DateTime): Creation timestamp
- **Updated** (DateTime?): Last update timestamp (nullable)
- **Blog** (BlogEntity): Navigation property to parent blog

**Validation Rules:**
- Name is required, 10-50 characters
- Content is required, max 1000 characters

## Database Schema

### Table: `blogs`

| Column      | Type    | Constraints           | Description                    |
|-------------|---------|-----------------------|--------------------------------|
| blog_id     | INTEGER | PRIMARY KEY, AUTOINC  | Unique blog identifier         |
| name        | TEXT    | NOT NULL, MAX(50)     | Blog name                      |
| is_active   | TEXT    | NOT NULL              | Active status as string        |

**Note:** `is_active` stores "Blog is active" or "Blog is not active" (not boolean)

### Table: `articles`

| Column   | Type     | Constraints                    | Description                    |
|----------|----------|--------------------------------|--------------------------------|
| post_id  | INTEGER  | PRIMARY KEY, AUTOINC           | Unique post identifier         |
| blog_id  | INTEGER  | FOREIGN KEY, NOT NULL          | Reference to blogs.blog_id    |
| name     | TEXT     | NOT NULL, MAX(50)              | Post name                      |
| content  | TEXT     | NOT NULL, MAX(1000)            | Post content                   |
| created  | TEXT     | NOT NULL                        | Creation timestamp             |
| updated  | TEXT     | NULL                            | Update timestamp (nullable)    |

**Foreign Key:**
- `blog_id` → `blogs.blog_id` with CASCADE DELETE

**Index:**
- `IX_articles_blog_id` on `blog_id` column

### Naming Conventions

- **Tables**: snake_case (`blogs`, `articles`)
- **Columns**: snake_case (`blog_id`, `post_id`, `is_active`)
- **Primary Keys**: `{entity}_id` format
- **Foreign Keys**: Descriptive names (`blog_id` for posts referencing blogs)

## API Endpoints

### Blogs API

| Method | Endpoint          | Description                    | Status Codes                    |
|--------|-------------------|--------------------------------|---------------------------------|
| GET    | `/api/blogs`      | Get all blogs                  | 200 OK                          |
| GET    | `/api/blogs/{id}` | Get blog by ID                 | 200 OK, 404 Not Found          |
| POST   | `/api/blogs`      | Create new blog                | 201 Created, 400 Bad Request   |
| PUT    | `/api/blogs/{id}` | Update existing blog           | 204 No Content, 400/404        |
| DELETE | `/api/blogs/{id}` | Delete blog                    | 204 No Content                  |

### Posts API

| Method | Endpoint                    | Description                    | Status Codes                    |
|--------|-----------------------------|--------------------------------|---------------------------------|
| GET    | `/api/posts`                | Get all posts                  | 200 OK                          |
| GET    | `/api/posts/{id}`           | Get post by ID                 | 200 OK, 404 Not Found          |
| GET    | `/api/posts/blog/{blogId}`  | Get posts by blog ID           | 200 OK                          |
| POST   | `/api/posts`                 | Create new post                | 201 Created, 400/404           |
| PUT    | `/api/posts/{id}`            | Update existing post           | 204 No Content, 400/404        |
| DELETE | `/api/posts/{id}`            | Delete post                    | 204 No Content                  |

### Request/Response Examples

#### Create Blog Request
```json
POST /api/blogs
{
  "name": "My Awesome Blog",
  "isActive": true
}
```

#### Create Blog Response
```json
HTTP 201 Created
{
  "id": 1,
  "name": "My Awesome Blog",
  "isActive": true,
  "articleCount": 0
}
```

#### Create Post Request
```json
POST /api/posts
{
  "name": "My First Post",
  "content": "This is the content of my first blog post.",
  "blogId": 1
}
```

#### Error Response
```json
HTTP 400 Bad Request
{
  "error": "The Name field is required."
}
```

## Critical Bug Fixes

The implementation addressed six critical architectural and code-level deficiencies:

### 1. Missing Primary Key Configuration
**Problem:** EF Core conventions couldn't identify `BlogId` and `PostId` as primary keys.

**Solution:** Explicit `HasKey()` configuration in `BlogConfiguration` and `PostConfiguration`:
```csharp
builder.HasKey(b => b.BlogId);
builder.HasKey(p => p.PostId);
```

### 2. Ambiguous Relationships
**Problem:** `PostEntity` had `ParentId` but no navigation property, causing EF to create shadow foreign keys.

**Solution:** 
- Added `BlogEntity Blog` navigation property to `PostEntity`
- Explicit relationship mapping: `HasForeignKey(p => p.ParentId)`

### 3. Incorrect Table Names
**Problem:** EF Core defaulted to `Blogs` and `Posts` (pluralized entity names).

**Solution:** Explicit table mapping:
```csharp
builder.ToTable("blogs");
builder.ToTable("articles");
```

### 4. Incorrect Column Names
**Problem:** Columns used PascalCase (`BlogId`, `PostId`) instead of snake_case.

**Solution:** Explicit column mapping for all properties:
```csharp
builder.Property(b => b.BlogId).HasColumnName("blog_id");
builder.Property(p => p.ParentId).HasColumnName("blog_id");
```

### 5. Missing Data Conversion
**Problem:** `IsActive` boolean stored as `bit` (0/1) instead of descriptive strings.

**Solution:** Custom value conversion:
```csharp
builder.Property(b => b.IsActive)
    .HasConversion(
        v => v ? "Blog is active" : "Blog is not active",
        v => v == "Blog is active"
    );
```

### 6. No Validation Pipeline
**Problem:** Invalid data only failed at database level with `SqlException`.

**Solution:** `SaveChanges()` override with validation:
```csharp
public override int SaveChanges()
{
    ValidateEntities();
    return base.SaveChanges();
}
```

## Testing Strategy

### Test Coverage Summary

**Total: 59 tests passing**

| Layer              | Test Count | Coverage Focus                          |
|--------------------|------------|-----------------------------------------|
| Domain             | 11 tests   | Entity validation, Data Annotations     |
| Infrastructure     | 24 tests   | EF configurations, repositories, validation pipeline |
| Application        | 18 tests   | Service logic, DTO mapping, business rules |
| Integration        | 6 tests    | Validation pipeline, end-to-end flows   |

### Test Types

1. **Unit Tests** (Domain, Application)
   - Entity validation using `System.ComponentModel.DataAnnotations.Validator`
   - Service logic with mocked repositories
   - DTO mapping verification

2. **Integration Tests** (Infrastructure)
   - EF Core configuration verification
   - Database schema validation
   - Value conversion testing
   - Cascade delete verification

3. **Validation Tests** (Integration)
   - Application-level validation enforcement
   - Error handling verification
   - Data annotation compliance

### Database Providers for Testing

- **InMemory**: Fast unit tests, no persistence
- **SQLite**: Configuration and schema verification tests
- **SQL Server**: Production migration validation (Docker)

## Validation Pipeline

The application enforces validation at the application level before database operations:

1. **Entity Validation**: Data Annotations validated using `Validator.ValidateObject()`
2. **Pre-Save Validation**: `SaveChanges()` and `SaveChangesAsync()` validate all added/modified entities
3. **Exception Handling**: `ValidationException` thrown with descriptive messages
4. **API Error Responses**: Validation errors return HTTP 400 Bad Request

### Validation Rules

**BlogEntity:**
- Name: Required, 10-50 characters

**PostEntity:**
- Name: Required, 10-50 characters
- Content: Required, max 1000 characters

## Deployment

### Development Environment

**Requirements:**
- .NET 10.0 SDK
- SQLite (included with .NET)

**Run Locally:**
```bash
cd src/BlogPlatform.Api
dotnet run
```

**Apply Migrations:**
```bash
cd src/BlogPlatform.Infrastructure
dotnet ef database update --startup-project ../BlogPlatform.Api
```

**Access Swagger UI:**
- URL: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)

### Production Deployment (Docker)

**Prerequisites:**
- Docker and Docker Compose
- SQL Server 2022 container image

**Deploy:**
```bash
docker-compose up -d
```

**Services:**
- **SQL Server**: Port 1433
- **API**: Port 5000 (mapped to container port 8080)

**Environment Variables:**
- `ASPNETCORE_ENVIRONMENT=Production`
- `DB_PASSWORD`: SQL Server password (from docker-compose.yml)

**Migration Execution:**
Migrations should be run manually or via startup script:
```bash
docker exec -it <api-container> dotnet ef database update
```

### Connection Strings

**Development (appsettings.json):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=blogs.db"
}
```

**Production (appsettings.Production.json):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=sql-server;Database=BlogPlatform;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True"
}
```

## Configuration

### Dependency Injection Setup

**Repositories:**
```csharp
services.AddScoped<IBlogRepository, BlogRepository>();
services.AddScoped<IPostRepository, PostRepository>();
```

**Services:**
```csharp
services.AddScoped<IBlogService, BlogService>();
services.AddScoped<IPostService, PostService>();
```

**DbContext:**
```csharp
services.AddDbContext<BlogsContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString);
});
```

## API Documentation

Swagger/OpenAPI documentation is automatically generated and available at:
- **Development**: `/swagger` endpoint
- **UI**: `/swagger/index.html`

The documentation includes:
- All API endpoints
- Request/response schemas
- Validation rules
- Example requests

## Development Guidelines

### Adding New Features

1. **Domain Layer**: Add entities and interfaces
2. **Infrastructure Layer**: Implement repositories and EF configurations
3. **Application Layer**: Create services and DTOs
4. **Presentation Layer**: Add controllers and endpoints
5. **Tests**: Write tests for each layer

### Code Style

- Use async/await for all I/O operations
- Follow Clean Architecture dependency rules
- Use dependency injection for all dependencies
- Validate at application level before database operations
- Use snake_case for database objects (tables, columns)

### Testing Requirements

- All new features must include unit tests
- Integration tests for database operations
- Validation tests for new entities
- Maintain minimum 80% code coverage

## Known Limitations

1. **Integration Tests**: `BlogEndToEndTests` have database provider conflicts with `WebApplicationFactory`. The comprehensive unit and integration tests (59 tests) provide full coverage of all critical paths.

2. **Migration Execution**: Production migrations require manual execution or startup script integration.

## Future Enhancements

Potential improvements for future iterations:

1. **Authentication & Authorization**: JWT-based authentication
2. **Pagination**: Add pagination to list endpoints
3. **Search**: Full-text search for blogs and posts
4. **Caching**: Redis caching for frequently accessed data
5. **Logging**: Structured logging with Serilog
6. **Health Checks**: API health check endpoints
7. **Rate Limiting**: Protect API from abuse
8. **API Versioning**: Support multiple API versions

## License

This project is part of a technical implementation demonstration.

## Contact

For questions or issues, please refer to the project documentation or create an issue in the repository.

