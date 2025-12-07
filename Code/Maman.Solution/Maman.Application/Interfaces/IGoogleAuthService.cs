using Google.Apis.Auth;
using Maman.Application.DTOs.Auth;

namespace Maman.Application.Interfaces;

public interface IGoogleAuthService
{
	//Task<UserDto> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken);
	Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken , CancellationToken cancellationToken);
}