# Project Brief

## Project Overview

Blog Platform is a production-ready SaaS blogging platform built with ASP.NET Core and Entity Framework Core, implementing Clean Architecture principles. The application provides a RESTful API for managing blogs and posts with comprehensive validation, proper database mapping, JWT-based authentication, role-based authorization, and full test coverage.

## Core Requirements

1. **Blog Management**: Users can create, read, update, and delete blogs
2. **Post Management**: Users can create, read, update, and delete posts within blogs
3. **Authentication**: Secure JWT-based authentication with dual-token system
4. **Authorization**: Role-based access control (Admin/User) with resource ownership
5. **Security**: Rate limiting, security headers, token blacklisting
6. **Testing**: Comprehensive unit and integration test coverage

## Architecture Principles

- **Clean Architecture**: Clear separation of concerns across four layers (Domain, Application, Infrastructure, Presentation)
- **Dependency Injection**: Extension methods for layer-based service registration
- **SOLID Principles**: Single responsibility, dependency inversion
- **Testability**: All layers designed for easy unit and integration testing

## Technology Stack

- .NET 10.0
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core 10.0
- JWT Authentication
- MSTest for testing
- Moq for mocking

## Key Features

- Dual-token authentication (15-min access tokens, 7-day refresh tokens)
- Role-based access control (Admin/User)
- Resource ownership (users can only edit their own content)
- Compromised token blacklist
- Rate limiting for API protection
- Security headers
- Comprehensive test coverage (126 tests)
