using Maman.Core.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Maman.Core.Entities.Tokens;

public class RefreshToken : BaseToken
{
	public bool IsRevoked { get; set; }

	public string? ReplacedByToken { get; set; }
	public string? RevokedByIp { get; set; }
	public DateTime? RevokedDate { get; set; }

	public string CreatedByIp { get; set; } = string.Empty;
	public string? UserAgent { get; set; }

	[JsonIgnore]
	public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
	[JsonIgnore]
	public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;

	public void Revoke(string? ipAddress = null)
	{
		IsRevoked = true;
		RevokedDate = DateTime.UtcNow;
		RevokedByIp = ipAddress;
	}

	public void MarkAsUsed()
	{
		IsUsed = true;
		UpdatedAt = DateTime.UtcNow;
	}
}