namespace BlogPlatform.Application.Services.Interfaces;

/// <summary>
/// Service for sending emails (stub implementation for future integration)
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email confirmation link to the specified email address
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="confirmationLink">The confirmation link</param>
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    
    /// <summary>
    /// Sends a password reset link to the specified email address
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="resetLink">The password reset link</param>
    Task SendPasswordResetAsync(string email, string resetLink);
    
    /// <summary>
    /// Sends a security alert when suspicious activity is detected
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="alertMessage">The security alert message</param>
    Task SendSecurityAlertAsync(string email, string alertMessage);
}

