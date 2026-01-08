# Blog Platform API

A RESTful API for managing blogs and posts, built with .NET 10.0 using Clean Architecture principles.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Local Development](#local-development)
  - [Docker Deployment](#docker-deployment)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Testing](#testing)
- [Configuration](#configuration)
- [Branch Information](#branch-information)

## Overview

Blog Platform API is a backend service that provides endpoints for managing blogs and their associated posts (articles). The application follows Clean Architecture principles, ensuring separation of concerns and maintainability.

### Key Capabilities

- **Blog Management**: Create, read, update, and delete blogs
- **Post Management**: Create, read, update, and delete posts within blogs
- **Data Validation**: Comprehensive validation at both entity and API levels
- **Database Flexibility**: SQLite for development, SQL Server for production
- **API Documentation**: Swagger/OpenAPI integration for interactive API exploration

## Architecture

The project follows Clean Architecture with four main layers:

```
BlogPlatform.sln
├── BlogPlatform.Domain          # Domain entities and interfaces
├── BlogPlatform.Application     # Business logic and DTOs
├── BlogPlatform.Infrastructure  # Data access and external services
└── BlogPlatform.Api            # Web API controllers and configuration
```

### Layer Responsibilities

- **Domain Layer**: Contains core business entities (`BlogEntity`, `PostEntity`) and repository interfaces. No dependencies on other layers.
- **Application Layer**: Contains business logic (services), DTOs for data transfer, and service interfaces. Depends only on Domain layer.
- **Infrastructure Layer**: Implements data access (Entity Framework Core), repositories, and database configurations. Depends on Domain and Application layers.
- **API Layer**: Contains controllers, dependency injection configuration, and application startup. Depends on all other layers.

## Technology Stack

- **.NET 10.0**: Target framework
- **ASP.NET Core Web API**: Web framework
- **Entity Framework Core 10.0.1**: ORM for data access
- **SQLite**: Development database
- **SQL Server**: Production database
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization support
- **MSTest**: Unit and integration testing framework

## Project Structure

```
backend-task/
├── src/
│   ├── BlogPlatform.Api/                    # Web API layer
│   │   ├── Controllers/                     # API controllers
│   │   │   ├── BlogsController.cs
│   │   │   └── PostsController.cs
│   │   ├── Program.cs                       # Application entry point
│   │   ├── appsettings.json                 # Configuration
│   │   └── BlogPlatform.Api.csproj
│   │
│   ├── BlogPlatform.Application/            # Application layer
│   │   ├── DTOs/                           # Data Transfer Objects
│   │   │   ├── BlogDto.cs
│   │   │   ├── PostDto.cs
│   │   │   ├── CreateBlogRequest.cs
│   │   │   ├── CreatePostRequest.cs
│   │   │   ├── UpdateBlogRequest.cs
│   │   │   └── UpdatePostRequest.cs
│   │   └── Services/                       # Business logic
│   │       ├── BlogService.cs
│   │       ├── PostService.cs
│   │       └── Interfaces/
│   │           ├── IBlogService.cs
│   │           └── IPostService.cs
│   │
│   ├── BlogPlatform.Domain/                 # Domain layer
│   │   ├── Entities/                      # Domain entities
│   │   │   ├── BlogEntity.cs
│   │   │   └── PostEntity.cs
│   │   └── Interfaces/                    # Repository interfaces
│   │       ├── IBlogRepository.cs
│   │       └── IPostRepository.cs
│   │
│   └── BlogPlatform.Infrastructure/        # Infrastructure layer
│       ├── Data/                          # DbContext
│       │   └── BlogsContext.cs
│       ├── Repositories/                  # Repository implementations
│       │   ├── BlogRepository.cs
│       │   └── PostRepository.cs
│       ├── Configurations/                # EF Core configurations
│       │   ├── BlogConfiguration.cs
│       │   └── PostConfiguration.cs
│       └── Migrations/                    # Database migrations
│
├── tests/                                  # Test projects
│   ├── BlogPlatform.Domain.Tests/
│   ├── BlogPlatform.Application.Tests/
│   ├── BlogPlatform.Infrastructure.Tests/
│   └── BlogPlatform.Integration.Tests/
│
├── Dockerfile                             # Docker build configuration
├── docker-compose.yml                     # Docker Compose configuration
└── BlogPlatform.sln                       # Solution file
```

## Features

### Blog Management

- **GET** `/api/blogs` - Retrieve all blogs
- **GET** `/api/blogs/{id}` - Retrieve a specific blog by ID
- **POST** `/api/blogs` - Create a new blog
- **PUT** `/api/blogs/{id}` - Update an existing blog
- **DELETE** `/api/blogs/{id}` - Delete a blog

**Blog Entity Properties:**
- `Id` (int): Unique identifier
- `Name` (string, 10-50 characters): Blog name
- `IsActive` (bool): Active status
- `ArticleCount` (int): Number of posts in the blog

### Post Management

- **GET** `/api/posts` - Retrieve all posts
- **GET** `/api/posts/{id}` - Retrieve a specific post by ID
- **GET** `/api/posts/blog/{blogId}` - Retrieve all posts for a specific blog
- **POST** `/api/posts` - Create a new post
- **PUT** `/api/posts/{id}` - Update an existing post
- **DELETE** `/api/posts/{id}` - Delete a post

**Post Entity Properties:**
- `Id` (int): Unique identifier
- `Name` (string, 10-50 characters): Post name
- `Content` (string, max 1000 characters): Post content
- `Created` (DateTime): Creation timestamp
- `Updated` (DateTime?): Last update timestamp
- `BlogId` (int): Parent blog identifier
- `BlogName` (string): Parent blog name

### Data Validation

The application enforces validation at multiple levels:

1. **Entity Level**: Data annotations on domain entities (`Required`, `StringLength`)
2. **DbContext Level**: Automatic validation before saving changes
3. **API Level**: Request validation and error handling

Validation errors return appropriate HTTP status codes:
- `400 Bad Request`: Validation errors
- `404 Not Found`: Resource not found
- `201 Created`: Successful creation
- `204 No Content`: Successful update/delete

## Getting Started

### Prerequisites

- **.NET 10.0 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker** (optional): For containerized deployment
- **SQL Server** (optional): For production database

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd backend-task
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** (if needed)
   
   Edit `src/BlogPlatform.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=blogs.db"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   cd src/BlogPlatform.Api
   dotnet ef database update --project ../BlogPlatform.Infrastructure
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   
   Navigate to `https://localhost:5152/swagger` (or the port configured in `launchSettings.json`)

### Docker Deployment

The project includes Docker support for containerized deployment.

#### Using Docker Compose (Recommended)

1. **Start services**
   ```bash
   docker-compose up -d
   ```

   This will:
   - Build the API Docker image
   - Start SQL Server container
   - Start the API container
   - Wait for SQL Server to be healthy before starting the API

2. **Access the API**
   
   The API will be available at `http://localhost:5000`

3. **Stop services**
   ```bash
   docker-compose down
   ```

#### Using Dockerfile

1. **Build the image**
   ```bash
   docker build -t blogplatform-api .
   ```

2. **Run the container**
   ```bash
   docker run -p 5000:8080 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e ConnectionStrings__DefaultConnection="<your-connection-string>" \
     blogplatform-api
   ```

**Note**: For production deployment, ensure you:
- Set appropriate connection strings
- Configure environment variables
- Use secure passwords for SQL Server
- Enable HTTPS
- Configure proper logging

## API Documentation

### Swagger/OpenAPI

When running in Development mode, Swagger UI is automatically available at `/swagger`. This provides:
- Interactive API exploration
- Request/response schemas
- Try-it-out functionality

### Example API Requests

#### Create a Blog
```http
POST /api/blogs
Content-Type: application/json

{
  "name": "My Tech Blog",
  "isActive": true
}
```

#### Create a Post
```http
POST /api/posts
Content-Type: application/json

{
  "name": "Introduction to Clean Architecture",
  "content": "Clean Architecture is a software design philosophy...",
  "blogId": 1
}
```

#### Get Posts by Blog
```http
GET /api/posts/blog/1
```

## Database

### Database Schema

The application uses Entity Framework Core with Code First migrations.

#### Tables

**blogs**
- `blog_id` (int, PK): Primary key
- `name` (string, 50): Blog name
- `is_active` (string): Active status (converted from bool)

**articles** (posts)
- `post_id` (int, PK): Primary key
- `blog_id` (int, FK): Foreign key to blogs
- `name` (string, 50): Post name
- `content` (string, 1000): Post content
- `created` (datetime): Creation timestamp
- `updated` (datetime, nullable): Update timestamp

#### Relationships

- One blog can have many posts (one-to-many)
- Cascade delete: Deleting a blog deletes all associated posts

### Database Providers

- **Development**: SQLite (file-based, no server required)
- **Production**: SQL Server (configured via connection string)

### Migrations

Migrations are located in `src/BlogPlatform.Infrastructure/Migrations/`.

**Create a new migration:**
```bash
cd src/BlogPlatform.Api
dotnet ef migrations add <MigrationName> --project ../BlogPlatform.Infrastructure
```

**Apply migrations:**
```bash
dotnet ef database update --project ../BlogPlatform.Infrastructure
```

## Testing

The project includes comprehensive test coverage across all layers:

### Test Projects

- **BlogPlatform.Domain.Tests**: Unit tests for domain entities
- **BlogPlatform.Application.Tests**: Unit tests for business logic and services
- **BlogPlatform.Infrastructure.Tests**: Unit tests for repositories and configurations
- **BlogPlatform.Integration.Tests**: End-to-end integration tests

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/BlogPlatform.Application.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Framework

- **MSTest**: Primary testing framework
- Tests are organized by layer and functionality

## Configuration

### Application Settings

Configuration files are located in `src/BlogPlatform.Api/`:

- **appsettings.json**: Base configuration (SQLite for development)
- **appsettings.Development.json**: Development-specific settings
- **appsettings.Production.json**: Production settings (SQL Server)

### Connection Strings

**Development (SQLite):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blogs.db"
  }
}
```

**Production (SQL Server):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server;Database=BlogPlatform;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True"
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
- `DB_PASSWORD`: SQL Server password (for Docker deployment)

## Branch Information

**Current Branch**: `01-Implentation-build-plan`

This branch contains the implementation of the Blog Platform API with:
- Complete CRUD operations for blogs and posts
- Clean Architecture implementation
- Entity Framework Core integration
- Comprehensive test coverage
- Docker support
- Swagger/OpenAPI documentation

### Recent Commits

- Added .gitignore file and updated assembly informational versions
- Implementation completed
- Enhanced isolation rules and process documentation
- Added optimized isolation rules and visual process maps

## Development Notes

### Key Design Decisions

1. **Clean Architecture**: Separation of concerns across layers ensures maintainability and testability
2. **Repository Pattern**: Abstracts data access, making it easy to swap implementations
3. **DTO Pattern**: Separates internal domain models from external API contracts
4. **Validation Pipeline**: Multi-level validation ensures data integrity
5. **Database Flexibility**: Support for both SQLite (dev) and SQL Server (prod)

### Entity Framework Configuration

- Custom entity configurations in `Infrastructure/Configurations/`
- Snake_case column naming convention
- Explicit foreign key relationships
- Cascade delete for blog-post relationships
- Custom value conversion for `IsActive` boolean field

### Future Enhancements

Potential improvements for future iterations:
- Authentication and authorization
- Pagination for list endpoints
- Filtering and sorting capabilities
- Soft delete functionality
- Audit logging
- Caching layer
- Rate limiting
- Health checks endpoint

## License

[Specify license if applicable]

## Contributing

[Add contribution guidelines if applicable]

## Support

[Add support information if applicable]
