using Google.Apis.Auth;
using Maman.Application.DTOs.Auth;
using Maman.Application.Interfaces;
using Maman.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maman.Application.Services.Utility;

public class GoogleAuthService : IGoogleAuthService
{
	private readonly string _clientId;
	private readonly GoogleAuthSettings _googleAuthSettings;

	public GoogleAuthService(IOptions<GoogleAuthSettings> googleAuthSettings)
	{
		_googleAuthSettings  = googleAuthSettings.Value;
		_clientId = _googleAuthSettings.ClientId
			?? throw new InvalidOperationException("Google Client ID not configured");
	}

	public Task<UserDto> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
	{
		var settings = new GoogleJsonWebSignature.ValidationSettings
		{
			Audience = new[] { _clientId }
		};

		return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
	}
}
