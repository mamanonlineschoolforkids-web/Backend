using Maman.Core.Entities.Auth;

namespace Maman.Application.Interfaces;

public interface IJwtTokenService
{
	string GenerateAccessToken(User user);
	Task<string> GenerateEmailVerificationTokenAsync();
	Task<string> GeneratePasswordResetTokenAsync();
	string GenerateRefreshToken();
	string GetJwtIdFromToken(string token);
	string? GetUserIdFromToken(string token);
	bool ValidateToken(string token);
}
