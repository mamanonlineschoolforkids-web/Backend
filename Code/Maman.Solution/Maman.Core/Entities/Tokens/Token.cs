using Maman.Core.Common;
using Maman.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;


namespace Maman.Core.Entities.Tokens;

public class Token : BaseEntity
{
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; }
	public string UserToken { get; set; }
	public TokenType TokenType { get; set; }
	public DateTime ExpiresAt { get; set; }

	public bool IsUsed { get; set; }
	public DateTime UsedAt { get; set; }

	public string CreatedByIp { get; set; }
	public string? UserAgent { get; set; }

	public bool IsRevoked { get; set; }
	public string? RevokedByIp { get; set; }
	public DateTime? RevokedDate { get; set; }
	public string? ReplacedByToken { get; set; }

	[JsonIgnore]
	public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
	[JsonIgnore]
	public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;

	public void Revoke(string? ipAddress = null)
	{
		IsRevoked = true;
		RevokedDate = DateTime.UtcNow;
		RevokedByIp = ipAddress;
		IsDeleted = true;
		DeletedAt = DateTime.UtcNow;
	}
	public void MarkAsUsed()
	{
		IsUsed = true;
		UsedAt = DateTime.UtcNow;
		IsDeleted = true;
		DeletedAt = DateTime.UtcNow;
	}
}
