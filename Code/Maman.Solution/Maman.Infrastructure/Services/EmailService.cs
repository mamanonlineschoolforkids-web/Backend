using MailKit.Security;
using Maman.Application.Interfaces;
using Maman.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using System.Net.Mail;

namespace Maman.Application.Services.Utility;

public class EmailService : IEmailService
{
	private readonly ILogger<EmailService> _logger;
	private readonly EmailSettings _emailSettings;

	public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
	{
		_emailSettings = emailSettings.Value;
		_logger = logger;
	}

	public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
	{
		try
		{
			using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
			{
				EnableSsl = _emailSettings.EnableSsl,
				Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
				Subject = subject,
				Body = body,
				IsBodyHtml = isHtml
			};

			mailMessage.To.Add(to);

			await client.SendMailAsync(mailMessage, cancellationToken);
			_logger.LogInformation("Email sent successfully to {Email}", to);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {Email}", to);
			throw;
		}
	}

	public async Task SendVerificationEmailAsync(string to, string userName, string verificationLink, CancellationToken cancellationToken = default)
	{
		var subject = "Verify Your Email Address";
		var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Hello {userName},</h2>
                <p>Thank you for registering! Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>Verify Email</a></p>
                <p>If you didn't create this account, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
                <br/>
                <p>Best regards,<br/>The Team</p>
            </body>
            </html>";

		await SendEmailAsync(to, subject, body, true, cancellationToken);
	}

	public async Task SendApplicationTourNotificationAsync(string email)
	{
		// TODO:: would be replaced with signalR
		_logger.LogInformation(" Tour Notification");
	}

	public async Task SendPasswordResetEmailAsync(string to, string userName, string resetLink, CancellationToken cancellationToken = default)
	{
		var subject = "Reset Your Password";
		var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Hello {userName},</h2>
                <p>We received a request to reset your password. Click the link below to create a new password:</p>
                <p><a href='{resetLink}' style='background-color: #2196F3; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email and your password will remain unchanged.</p>
                <p>This link will expire in 1 hour.</p>
                <br/>
                <p>Best regards,<br/>The Team</p>
            </body>
            </html>";

		await SendEmailAsync(to, subject, body, true, cancellationToken);
	}

	public async Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
	{
		var subject = "Welcome to Our Platform!";
		var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Welcome {userName}!</h2>
                <p>Your email has been verified successfully. Thank you for joining us!</p>
                <p>You can now access all features of our platform.</p>
                <br/>
                <p>Best regards,<br/>The Team</p>
            </body>
            </html>";

		await SendEmailAsync(to, subject, body, true, cancellationToken);
	}

	public async Task SendAccountDeletionEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
	{
		var subject = "Account Deletion Request";
		var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Hello {userName},</h2>
                <p>We've received your request to delete your account.</p>
                <p>Your account will be permanently deleted in 30 days. If you change your mind, you can restore your account before then.</p>
                <p>If you didn't request this, please contact our support immediately.</p>
                <br/>
                <p>Best regards,<br/>The Team</p>
            </body>
            </html>";

		await SendEmailAsync(to, subject, body, true, cancellationToken);
	}

	public async Task SendTwoFactorCodeAsync(string email, string code)
	{
		var subject = "Your Two-Factor Authentication Code";
		var body = $@"
            <h2>Two-Factor Authentication</h2>
            <p>Your verification code is:</p>
            <h1 style='font-size: 32px; color: #007bff;'>{code}</h1>
            <p>This code will expire in 5 minutes.</p>
            <p>If you didn't request this code, please secure your account immediately.</p>
        ";

		await SendEmailAsync(email, subject, body);
	}

	public async Task SendPasswordChangedNotificationAsync(string email)
	{
		var subject = "Password Changed Successfully";
		var body = $@"
            <h2>Password Changed</h2>
            <p>Your password has been changed successfully.</p>
            <p>If you didn't make this change, please contact our support team immediately.</p>
            <p>Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
        ";

		await SendEmailAsync(email, subject, body);
	}

	public async Task SendAccountLockedNotificationAsync(string email, DateTime lockoutEnd)
	{
		var subject = "Account Temporarily Locked";
		var body = $@"
            <h2>Account Locked</h2>
            <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
            <p>Your account will be automatically unlocked at: {lockoutEnd:yyyy-MM-dd HH:mm:ss} UTC</p>
            <p>If this wasn't you, please contact our support team immediately.</p>
        ";

		await SendEmailAsync(email, subject, body);
	}

	public async Task SendAccountRestoreEmailAsync(string email)
	{
		throw new NotImplementedException();
	}

}

