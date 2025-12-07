using Maman.Application.Interfaces;
using Maman.Core.Entities.Auth;
using Maman.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Maman.Application.Services.Utility;

public class JwtTokenService : IJwtTokenService
{
	private readonly JwtSettings _jwtSettings;
	private readonly TokenValidationParameters _tokenValidationParameters;

	public JwtTokenService(IOptions<JwtSettings> jwtSettings)
	{
		_jwtSettings = jwtSettings.Value;
		_tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = _jwtSettings.Issuer,
			ValidAudience = _jwtSettings.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
			ClockSkew = TimeSpan.Zero
		};
	}

	public string GenerateAccessToken(User user)
	{
		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new(JwtRegisteredClaimNames.Email, user.Email),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Name, user.Name),
			new("preferredLanguage", user.PreferredLanguage.ToString())
		};

		// Add role claims
		claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));


		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Issuer,
			audience: _jwtSettings.Audience,
			claims: claims,
			expires: expires,
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var randomBytes = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomBytes);
		return Convert.ToBase64String(randomBytes);
	}

	public string GetJwtIdFromToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var jwtToken = tokenHandler.ReadJwtToken(token);
		return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
			   ?? throw new InvalidOperationException("Token does not contain JTI claim");
	}

	public string? GetUserIdFromToken(string token)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
			var jwtToken = (JwtSecurityToken)validatedToken;
			return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
		}
		catch
		{
			return null;
		}
	}

	public async Task<string> GenerateEmailVerificationTokenAsync()
	{
		return await Task.FromResult(GenerateSecureToken());
	}

	public async Task<string> GeneratePasswordResetTokenAsync()
	{
		return await Task.FromResult(GenerateSecureToken());
	}

	private string GenerateSecureToken()
	{
		var randomBytes = new byte[32];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomBytes);
		return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_");
	}

	public bool ValidateToken(string token)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
			return true;
		}
		catch
		{
			return false;
		}
	}
}

