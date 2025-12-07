using BCrypt.Net;
using Maman.Application.DTOs.Auth;
using Maman.Application.DTOs.Common;
using Maman.Application.Interfaces;
using Maman.Core.Entities.Auth;
using Maman.Core.Entities.Tokens;
using Maman.Core.Enums;
using Maman.Core.Interfaces;
using Maman.Core.Settings;
using Maman.Core.Specifications;
using Maman.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Maman.Application.Services;
public class AuthService : IAuthService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IJwtTokenService _jwtService;
	private readonly IEmailService _emailService;
	private readonly IGoogleAuthService _googleAuthService;
	private readonly ITwoFactorService _twoFactorService;
	private readonly ICacheService _cacheService;
	private readonly IStringLocalizer<SharedResource> _localizer;
	private readonly ILogger<AuthService> _logger;
	private readonly AuthSettings _authSettings;

	public AuthService(
		IUnitOfWork unitOfWork,
		IJwtTokenService jwtService,
		IEmailService emailService,
		IGoogleAuthService googleAuthService,
		ITwoFactorService twoFactorService,
		ICacheService cacheService,
		IStringLocalizer<SharedResource> localizer,
		ILogger<AuthService> logger,
		IOptions<AuthSettings> authSettings)
	{
		_unitOfWork = unitOfWork;
		_jwtService = jwtService;
		_emailService = emailService;
		_googleAuthService = googleAuthService;
		_twoFactorService = twoFactorService;
		_cacheService = cacheService;
		_localizer = localizer;
		_logger = logger;
		_authSettings = authSettings.Value;
	}

	public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
	{
		try
		{
			if (await _unitOfWork.Users.FindOneAsync(new UserByEmailAndPhoneSpecification(request.Email , request.PhoneNumber) , cancellationToken) is not null)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["EmailOrPhoneNumberAlreadyExists"]);
			}

			var user = new User
			{
				Name = request.Name,
				Email = request.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
				Country = request.Country,
				PhoneNumber = request.PhoneNumber,
				Role =  request.Role ,
				IsEmailVerified = false,
				FirstLogin = true
			};

			InitializeUserProfile(user, request.Role);

			var verificationToken = await _jwtService.GenerateEmailVerificationTokenAsync();

			var emailToken = new Token
			{
				UserId = user.Id,
				UserToken =verificationToken,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.VerifyEmailTokenExpirationMinutes),
				TokenType = TokenType.VerifyEmail
			};

			await _unitOfWork.BeginTransactionAsync(cancellationToken);
			try
			{
				await _unitOfWork.Users.AddAsync(user, cancellationToken);
				await _unitOfWork.Tokens.AddAsync(emailToken, cancellationToken);
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				_logger.LogError("Database Error while registering : {Email} {Message}", user.Email ,ex.Message );
			}
			await _unitOfWork.CommitTransactionAsync(cancellationToken);


			var verificationLink = $"{_authSettings.FrontendUrl}/verify-email?token={verificationToken}";
			await _emailService.SendVerificationEmailAsync(user.Email, user.Name, _authSettings.VerifyEmailTokenExpirationMinutes , verificationLink, cancellationToken);


			_logger.LogInformation("User registered successfully: {Email}", user.Email);

			return ApiResponseDto<AuthResponseDto>.SuccessResponse(
				null!,
				_localizer["RegistrationSuccessful"]
			);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during registration for {Email}", request.Email);
			return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["RegistrationFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.FindOneAsync(new UserByEmailSpecification(email), cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			if (user.IsEmailVerified)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["EmailAlreadyVerified"]);
			}

			var verificationToken = await _jwtService.GenerateEmailVerificationTokenAsync();
			
			var tokenEntity = new Token
			{
				UserId = user.Id,
				UserToken = verificationToken,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.VerifyEmailTokenExpirationMinutes),
				TokenType = TokenType.VerifyEmail
			};


			await _unitOfWork.BeginTransactionAsync(cancellationToken);

			try
			{
				await _unitOfWork.Tokens.InvalidateAllUserTokensAsync(user.Id, TokenType.VerifyEmail, cancellationToken);
				await _unitOfWork.Tokens.AddAsync(tokenEntity, cancellationToken);
			}
			catch(Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				_logger.LogError("Database Error resend verification to : {Email} {Message}", user.Email, ex.Message);
			}

			await _unitOfWork.CommitTransactionAsync(cancellationToken);

			var verificationLink = $"{_authSettings.FrontendUrl}/verify-email?token={verificationToken}";
			await _emailService.SendVerificationEmailAsync(user.Email, user.Name,_authSettings.VerifyEmailTokenExpirationMinutes, verificationLink, cancellationToken);

			_logger.LogInformation("Verification email resent to: {Email}", email);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["VerificationEmailResent"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error resending verification email to {Email}", email);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["ResendVerificationFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> VerifyEmailAsync(VerifyEmailDto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var tokenEntity = await _unitOfWork.Tokens.FindOneAsync(new ValidTokenSpecifications(request.Token , TokenType.VerifyEmail), cancellationToken);

			if (tokenEntity == null)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["InvalidOrExpiredToken"]);
			}

			var user = await _unitOfWork.Users.GetByIdAsync(tokenEntity.UserId, cancellationToken);

			if (user == null)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			if (user.IsEmailVerified)
			{
				return ApiResponseDto<bool>.SuccessResponse(true, _localizer["EmailAlreadyVerified"]);
			}


			user.VerifyEmail();
			tokenEntity.MarkAsUsed();

			await _unitOfWork.BeginTransactionAsync(cancellationToken);

			try
			{
				await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

				await _unitOfWork.Tokens.UpdateAsync(tokenEntity, cancellationToken);
			}
			catch(Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				_logger.LogError("Database Error while verifying : {Email} {Message}", user.Email, ex.Message);
			}

			await _unitOfWork.CommitTransactionAsync(cancellationToken);

			await _emailService.SendWelcomeEmailAsync(user.Email, user.Name, cancellationToken);

			await _cacheService.SetAsync($"user:{user.Id}", user, TimeSpan.FromHours(1), cancellationToken);


			_logger.LogInformation("Email verified for user: {UserId}", user.Id);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["EmailVerifiedSuccessfully"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error verifying email");
			return ApiResponseDto<bool>.ErrorResponse(_localizer["EmailVerificationFailed"]);
		}
	}

	public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.FindOneAsync(new UserByEmailSpecification(request.Email) , cancellationToken);

			if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				_logger.LogWarning("Failed login attempt for {Email}", request.Email);

				if (user is not null)
				{
					await HandleFailedLogin(user, cancellationToken);

					await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
				}

				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["InvalidCredentials"]);
			}


			if (user.IsLockedOut)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(
					_localizer["AccountLockedOut", user.LockoutEndDate!.Value.ToString("yyyy-MM-dd HH:mm:ss")]
				);
			}


			if (user.IsDeleted)
			{
				await _emailService.SendAccountRestoreEmailAsync(user.Email);
			}

			if (user.IsSuspended)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["AccountSuspended"]);
			}

			if (!user.IsEmailVerified)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["EmailNotVerified"]);
			}

			if (user.TwoFactorEnabled)
			{
				if (string.IsNullOrEmpty(request.TwoFactorCode))
				{
					return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["TwoFactorCodeRequired"]);
				}

				if (!_twoFactorService.ValidateCode(user.TwoFactorSecret!, request.TwoFactorCode))
				{
					await HandleFailedLogin(user, cancellationToken);
					return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["InvalidTwoFactorCode"]);
				}
			}

			user.ResetFailedLoginAttempts();

			if (user.FirstLogin)
			{
				await _emailService.SendApplicationTourNotificationAsync(user.Email);
				user.FirstLogin = false;
			}

			user.LastLogin = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			var accessToken = _jwtService.GenerateAccessToken(user);
			var refreshToken = _jwtService.GenerateRefreshToken();

			var refreshTokenEntity = new Token
			{
				UserId = user.Id,
				UserToken = refreshToken,
				TokenType = TokenType.Refresh,
				ExpiresAt = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpirationDays),
				CreatedByIp = ipAddress,
				UserAgent = userAgent
			};

			await _unitOfWork.Tokens.AddAsync(refreshTokenEntity, cancellationToken);

			await _cacheService.SetAsync($"user:{user.Id}", user, TimeSpan.FromHours(1), cancellationToken);

			_logger.LogInformation("User logged in successfully: {Email}", user.Email);

			return ApiResponseDto<AuthResponseDto>.SuccessResponse(new AuthResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.AccessTokenExpirationMinutes),
				User = MapToUserDto(user)
			}, _localizer["LoginSuccessful"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during login for {Email}", request.Email);
			return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["LoginFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> RequestPasswordResetAsync(RequestPasswordResetDto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.FindOneAsync(new UserByEmailSpecification(request.Email), cancellationToken);

			if (user == null)
			{
				_logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
				return ApiResponseDto<bool>.SuccessResponse(true, _localizer["PasswordResetEmailSent"]);
			}

			if (user.IsDeleted || user.IsSuspended)
			{
				return ApiResponseDto<bool>.SuccessResponse(true, _localizer["PasswordResetEmailSent"]);
			}

			var resetToken = await _jwtService.GeneratePasswordResetTokenAsync();
			var tokenEntity = new Token
			{
				UserId = user.Id,
				UserToken = resetToken,
				ExpiresAt = DateTime.UtcNow.AddHours(1),
				TokenType = TokenType.ResetPassword
			};

			await _unitOfWork.BeginTransactionAsync(cancellationToken);

			try
			{
				await _unitOfWork.Tokens.InvalidateAllUserTokensAsync(user.Id,TokenType.ResetPassword, cancellationToken);
				await _unitOfWork.Tokens.AddAsync(tokenEntity, cancellationToken);
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				_logger.LogError("Database Error while request resting password : {Email} {Message}", user.Email, ex.Message);
			}

			await _unitOfWork.CommitTransactionAsync(cancellationToken);



			var resetLink = $"{_authSettings.FrontendUrl}/reset-password?token={resetToken}";
			await _emailService.SendPasswordResetEmailAsync(user.Email, user.Name, resetLink, cancellationToken);

			_logger.LogInformation("Password reset requested for: {Email}", user.Email);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["PasswordResetEmailSent"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error requesting password reset for {Email}", request.Email);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["PasswordResetRequestFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto request, string ipAddress, CancellationToken cancellationToken = default)
	{
		try
		{

			var tokenEntity = await _unitOfWork.Tokens.FindOneAsync(new ValidTokenSpecifications(request.Token , TokenType.ResetPassword));

			if (tokenEntity == null)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["InvalidOrExpiredToken"]);
			}

			var user = await _unitOfWork.Users.GetByIdAsync(tokenEntity.UserId, cancellationToken);

			if (user == null || user.IsDeleted|| user.IsSuspended)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

			user.ResetFailedLoginAttempts();

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			tokenEntity.MarkAsUsed();

			await _unitOfWork.Tokens.UpdateAsync(tokenEntity, cancellationToken);

			await _unitOfWork.Tokens.RevokeAllUserTokensAsync(user.Id, ipAddress, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{user.Id}", cancellationToken);

			_logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["PasswordResetSuccessful"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error resetting password");
			return ApiResponseDto<bool>.ErrorResponse(_localizer["PasswordResetFailed"]);
		}
	}

	public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
	{
		try
		{
			var tokenEntity =await _unitOfWork.Tokens.FindOneAsync(new ValidTokenSpecifications(refreshToken , TokenType.Refresh), cancellationToken);

			if (tokenEntity == null || !tokenEntity.IsActive)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["InvalidRefreshToken"]);
			}

			var user = await _unitOfWork.Users.GetByIdAsync(tokenEntity.UserId, cancellationToken);

			if (user == null || user.IsDeleted || user.IsSuspended)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Generate new tokens
			var accessToken = _jwtService.GenerateAccessToken(user);

			return ApiResponseDto<AuthResponseDto>.SuccessResponse(new AuthResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.AccessTokenExpirationMinutes),
				User = MapToUserDto(user)
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error refreshing token");
			return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["TokenRefreshFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		try
		{
			await _unitOfWork.Tokens.RevokeTokenAsync(refreshToken, cancellationToken);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["TokenRevoked"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error revoking token");
			return ApiResponseDto<bool>.ErrorResponse(_localizer["TokenRevokeFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> LogoutAsync(string userId, string ipAddress, CancellationToken cancellationToken = default)
	{
		try
		{
			await _unitOfWork.Tokens.RevokeAllUserTokensAsync(userId, ipAddress, cancellationToken);
			await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

			_logger.LogInformation("User logged out: {UserId}", userId);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["LogoutSuccessful"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during logout for user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["LogoutFailed"]);
		}
	}
	public async Task<ApiResponseDto<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
	{
		try
		{
			var payload = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken, cancellationToken);

			if (payload is null)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["InvalidGoogleToken"]);
			}

			var user = await _unitOfWork.Users.FindOneAsync(new UserByGoogleIdSpecification(payload.Subject), cancellationToken);

			if (user == null)
			{
				user = await _unitOfWork.Users.FindOneAsync(new UserByEmailSpecification(payload.Email), cancellationToken);
			}

			if (user == null)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["NoAccountFound"]);
			}

			if (user.IsDeleted || user.IsSuspended)
			{
				return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["AccountNotActive"]);
			}

			user.LastLogin = DateTime.UtcNow;
			user.UpdatedAt = DateTime.UtcNow;
		
			if (user.GoogleId == null && user.Email == payload.Email)
			{
				user.GoogleId = payload.Subject;
			}

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			var accessToken = _jwtService.GenerateAccessToken(user);
			var refreshToken = _jwtService.GenerateRefreshToken();

			var refreshTokenEntity = new Token
			{
				UserId = user.Id,
				UserToken = refreshToken,
				ExpiresAt = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpirationDays),
				CreatedByIp = ipAddress,
				UserAgent = userAgent
			};

			await _unitOfWork.Tokens.AddAsync(refreshTokenEntity, cancellationToken);
			await _cacheService.SetAsync($"user:{user.Id}", user, TimeSpan.FromHours(1), cancellationToken);

			_logger.LogInformation("Google login successful: {Email}", user.Email);

			// 9. Return success response
			return ApiResponseDto<AuthResponseDto>.SuccessResponse(new AuthResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.AccessTokenExpirationMinutes),
				User = MapToUserDto(user)
			}, _localizer["LoginSuccessful"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during Google login");
			return ApiResponseDto<AuthResponseDto>.ErrorResponse(_localizer["GoogleLoginFailed"]);
		}
	}

	public async Task<ApiResponseDto<TwoFactorSetupResponseDto>> Enable2FAAsync(string userId, Enable2FADto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<TwoFactorSetupResponseDto>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Verify password
			if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				return ApiResponseDto<TwoFactorSetupResponseDto>.ErrorResponse(_localizer["InvalidPassword"]);
			}

			if (user.TwoFactorEnabled)
			{
				return ApiResponseDto<TwoFactorSetupResponseDto>.ErrorResponse(_localizer["TwoFactorAlreadyEnabled"]);
			}

			// Generate 2FA secret
			var secret = _twoFactorService.GenerateSecret();
			var qrCodeUrl = _twoFactorService.GenerateQrCodeUrl(user.Email, secret, "YourAppName");
			var manualEntryKey = _twoFactorService.GetManualEntryKey(secret);

			// Save secret temporarily (will be enabled after verification)
			user.TwoFactorEnabled = true;
			user.TwoFactorSecret = secret;
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			_logger.LogInformation("2FA setup initiated for user: {UserId}", userId);

			return ApiResponseDto<TwoFactorSetupResponseDto>.SuccessResponse(new TwoFactorSetupResponseDto
			{
				Secret = secret,
				QrCodeUrl = qrCodeUrl,
				ManualEntryKey = manualEntryKey
			}, _localizer["TwoFactorSetupInitiated"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error enabling 2FA for user {UserId}", userId);
			return ApiResponseDto<TwoFactorSetupResponseDto>.ErrorResponse(_localizer["TwoFactorEnableFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> Verify2FAAsync(string userId, Verify2FADto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			if (string.IsNullOrEmpty(user.TwoFactorSecret))
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["TwoFactorNotSetup"]);
			}

			// Validate code
			if (!_twoFactorService.ValidateCode(user.TwoFactorSecret, request.Code))
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["InvalidTwoFactorCode"]);
			}

			// Enable 2FA
			user.TwoFactorEnabled = true;
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{user.Id}", cancellationToken);

			_logger.LogInformation("2FA enabled successfully for user: {UserId}", userId);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["TwoFactorEnabled"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error verifying 2FA for user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["TwoFactorVerifyFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> Disable2FAAsync(string userId, Disable2FADto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["TwoFactorNotEnabled"]);
			}

			// Validate code before disabling
			if (!_twoFactorService.ValidateCode(user.TwoFactorSecret, request.Code))
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["InvalidTwoFactorCode"]);
			}

			// Disable 2FA
			user.TwoFactorEnabled = false;
			user.TwoFactorSecret = null;
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{user.Id}", cancellationToken);

			_logger.LogInformation("2FA disabled for user: {UserId}", userId);
			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["TwoFactorDisabled"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error disabling 2FA for user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["TwoFactorDisableFailed"]);
		}
	}

	private async Task HandleFailedLogin(User user, CancellationToken cancellationToken)
	{
		user.FailedLoginAttempts++;
		user.LastFailedLogin = DateTime.UtcNow;

		if (user.FailedLoginAttempts >= _authSettings.MaxFailedLoginAttempts)
		{
			user.LockoutEndDate = DateTime.UtcNow.AddMinutes(_authSettings.LockoutDurationMinutes);

			await _emailService.SendAccountLockedNotificationAsync(user.Email, user.LockoutEndDate.Value);

			_logger.LogWarning("Account locked due to failed login attempts: {Email}", user.Email);
		}

		await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
	}

	private void InitializeUserProfile(User user, UserRole role)
	{
		switch (role)
		{
			case UserRole.Student:
				user.StudentProfile = new StudentProfile();
				break;
			case UserRole.Parent:
				user.ParentProfile = new ParentProfile();
				break;
			case UserRole.ServiceProvider:
				user.ServiceProviderProfile = new ServiceProviderProfile();
				break;
			case UserRole.Admin:
				user.AdminProfile = new AdminProfile
				{
					Permissions = new Dictionary<string, Permissions>()
				};
				break;
		}
	}

	private UserDto MapToUserDto(User user)
	{
		return new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			Country = user.Country,
			PhoneNumber = user.PhoneNumber,
			IsEmailVerified = user.IsEmailVerified,
			ProfilePictureUrl = user.ProfilePictureUrl,
			Role = user.Role,
			LastLogin = user.LastLogin,
			Status = user.Status.ToString(),
			DisplayCalendar = user.DisplayCalendar.ToString(),
			PreferredLanguage = user.PreferredLanguage.ToString(),
			TwoFactorEnabled = user.TwoFactorEnabled,
			CreatedAt = user.CreatedAt
		};
	}

}



