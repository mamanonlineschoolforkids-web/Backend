namespace Maman.Application.Interfaces;

public interface IEmailService
{
	Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
	Task SendVerificationEmailAsync(string to, string userName,int expiry , string verificationLink, CancellationToken cancellationToken = default);
	Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default);
	Task SendApplicationTourNotificationAsync(string email);
	Task SendPasswordResetEmailAsync(string to, string userName, string resetLink, CancellationToken cancellationToken = default);
	Task SendAccountDeletionEmailAsync(string to, string userName, CancellationToken cancellationToken = default);
	Task SendTwoFactorCodeAsync(string email, string code);
	Task SendPasswordChangedNotificationAsync(string email);
	Task SendAccountLockedNotificationAsync(string email, DateTime lockoutEnd);
	Task SendAccountRestoreEmailAsync(string email);

}
