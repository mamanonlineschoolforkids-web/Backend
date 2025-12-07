using Maman.Application.DTOs.Auth;
using Maman.Application.DTOs.Common;

namespace Maman.Application.Interfaces;

public interface IAuthService
{
	Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
	Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> LogoutAsync(string userId, string ipAddress , CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> RequestPasswordResetAsync(RequestPasswordResetDto request, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto request, string ipAddress , CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> VerifyEmailAsync(VerifyEmailDto request, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<TwoFactorSetupResponseDto>> Enable2FAAsync(string userId, Enable2FADto request, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> Verify2FAAsync(string userId, Verify2FADto request, CancellationToken cancellationToken = default);

	Task<ApiResponseDto<bool>> Disable2FAAsync(string userId, Disable2FADto request, CancellationToken cancellationToken = default);

}
