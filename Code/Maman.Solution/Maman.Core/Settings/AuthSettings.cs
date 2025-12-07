namespace Maman.Core.Settings;

public class AuthSettings
{
	public string FrontendUrl { get; set; } = string.Empty;
	public int AccessTokenExpirationMinutes { get; set; }
	public int RefreshTokenExpirationDays { get; set; }
	public int VerifyEmailTokenExpirationMinutes { get; set; }
	public int ResetPasswordTokenExpirationMinutes { get; set; }
	public int MaxFailedLoginAttempts { get; set; } = 5;
	public int LockoutDurationMinutes { get; set; } = 15;
}
