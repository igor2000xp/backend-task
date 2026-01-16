using BlogPlatform.Application.Authorization;
using BlogPlatform.Application.Services;
using BlogPlatform.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Application;

public static class DependencyInjection
{
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
}
