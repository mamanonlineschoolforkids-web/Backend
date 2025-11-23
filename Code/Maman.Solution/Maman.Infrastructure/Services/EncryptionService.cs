using Maman.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Maman.Application.Services.Utility;

public class EncryptionService : IEncryptionService
{
	private readonly byte[] _key;
	private readonly byte[] _iv;

	public EncryptionService(IConfiguration configuration)
	{
		var encryptionKey = configuration["EncryptionSettings:Key"]
			?? throw new InvalidOperationException("Encryption key not configured");

		using var sha256 = SHA256.Create();
		_key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
		_iv = new byte[16]; // Use a fixed IV for simplicity, or derive from key
		Array.Copy(_key, _iv, 16);
	}

	public string Encrypt(string plainText)
	{
		if (string.IsNullOrEmpty(plainText))
			return plainText;

		using var aes = Aes.Create();
		aes.Key = _key;
		aes.IV = _iv;

		var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

		using var msEncrypt = new MemoryStream();
		using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
		using (var swEncrypt = new StreamWriter(csEncrypt))
		{
			swEncrypt.Write(plainText);
		}

		return Convert.ToBase64String(msEncrypt.ToArray());
	}

	public string Decrypt(string cipherText)
	{
		if (string.IsNullOrEmpty(cipherText))
			return cipherText;

		using var aes = Aes.Create();
		aes.Key = _key;
		aes.IV = _iv;

		var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

		using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
		using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
		using var srDecrypt = new StreamReader(csDecrypt);

		return srDecrypt.ReadToEnd();
	}
}