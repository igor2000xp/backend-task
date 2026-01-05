using BlogPlatform.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Application.Services;

/// <summary>
/// Stub implementation of email service for future integration.
/// Logs email operations instead of sending actual emails.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // TODO: Implement actual email sending when email service is configured
        _logger.LogInformation(
            "[EMAIL STUB] Sending email confirmation to {Email}. Confirmation link: {Link}",
            email, confirmationLink);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendPasswordResetAsync(string email, string resetLink)
    {
        // TODO: Implement actual email sending when email service is configured
        _logger.LogInformation(
            "[EMAIL STUB] Sending password reset to {Email}. Reset link: {Link}",
            email, resetLink);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendSecurityAlertAsync(string email, string alertMessage)
    {
        // TODO: Implement actual email sending when email service is configured
        _logger.LogInformation(
            "[EMAIL STUB] Sending security alert to {Email}. Message: {Message}",
            email, alertMessage);
        
        return Task.CompletedTask;
    }
}

