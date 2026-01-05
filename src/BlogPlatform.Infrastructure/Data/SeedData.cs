using BlogPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Data;

/// <summary>
/// Seeds initial data for the application including roles and default users
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<BlogsContext>>();

        // Create roles
        await CreateRoleAsync(roleManager, "Admin", logger);
        await CreateRoleAsync(roleManager, "User", logger);

        // Create admin user
        await CreateUserAsync(
            userManager,
            email: "admin@blogplatform.com",
            fullName: "System Administrator",
            password: "Admin@123456",
            role: "Admin",
            logger);

        // Create test user
        await CreateUserAsync(
            userManager,
            email: "user@blogplatform.com",
            fullName: "Test User",
            password: "User@123456",
            role: "User",
            logger);
    }

    private static async Task CreateRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName,
        ILogger logger)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                logger.LogInformation("Created role: {RoleName}", roleName);
            }
            else
            {
                logger.LogError("Failed to create role {RoleName}: {Errors}", 
                    roleName, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role,
        ILogger logger)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            logger.LogDebug("User {Email} already exists, skipping creation", email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("Created user: {Email} with role: {Role}", email, role);
        }
        else
        {
            logger.LogError("Failed to create user {Email}: {Errors}",
                email,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

