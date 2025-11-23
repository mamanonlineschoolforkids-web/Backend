using Maman.Application.Interfaces;
using OtpNet;

namespace Maman.Application.Services.Utility;

public class TwoFactorService : ITwoFactorService
{
	public string GenerateSecret()
	{
		var key = KeyGeneration.GenerateRandomKey(20);
		return Base32Encoding.ToString(key);
	}

	public string GenerateQrCodeUrl(string email, string secret, string issuer)
	{
		var otpUrl = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
		return otpUrl;
	}

	public bool ValidateCode(string secret, string code)
	{
		var totp = new Totp(Base32Encoding.ToBytes(secret));
		return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
	}

	public string GetManualEntryKey(string secret)
	{
		return secret.ToUpper().Insert(4, " ").Insert(9, " ").Insert(14, " ").Insert(19, " ");
	}
}
