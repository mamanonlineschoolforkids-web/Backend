using Maman.Core.Common;
using Maman.Core.Enums;
using Maman.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading;


namespace Maman.Core.Entities.Auth;
public class User : AuditableEntity
{
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty; 
	public string PasswordHash { get; set; } = string.Empty;
	public string Country { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty; 
	public bool IsEmailVerified { get; set; }
	public string? GoogleId { get; set; }
	public bool FirstLogin { get; set; } = true;
	public string? ProfilePictureUrl { get; set; }

	public UserRole Role { get; set; } = new();
	public DateTime LastLogin { get; set; }
	public UserStatus Status { get; set; } = UserStatus.Active;
	public DateTime? DeletionRequestedAt { get; set; }
	public Dictionary<string, DateTime>? LastActionAt { get; set; }
	public CalendarType DisplayCalendar { get; set; } = CalendarType.Gregorian;
	public PreferredLanguage PreferredLanguage { get; set; } = PreferredLanguage.Ar;

	// Security fields
	public int FailedLoginAttempts { get; set; }
	public DateTime? LastFailedLogin { get; set; }
	public DateTime? LockoutEndDate { get; set; }


	// 2FA
	public bool TwoFactorEnabled { get; set; }
	public string? TwoFactorSecret { get; set; }


	[BsonIgnore]
	public bool IsLockedOut => LockoutEndDate.HasValue && LockoutEndDate.Value > DateTime.UtcNow;
	[BsonIgnore]
	public bool IsSuspended => Status == UserStatus.Suspended;


	public StudentProfile? StudentProfile { get; set; }
	public ParentProfile? ParentProfile { get; set; }
	public ServiceProviderProfile? ServiceProviderProfile { get; set; }
	public AdminProfile? AdminProfile { get; set; }


	public void ResetFailedLoginAttempts()
	{
		FailedLoginAttempts = 0;
		LastFailedLogin = null;
		LockoutEndDate = null;
		LastLogin = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
		LockoutEndDate = null;
		LockoutEndDate = null;
	}

	public void SoftDelete(string? deletedBy = null)
	{
		IsDeleted = true;
		DeletedAt = DateTime.UtcNow;
		DeletedBy = deletedBy;
		Status = UserStatus.Deleted;
	}

	public void VerifyEmail()
	{
		IsEmailVerified = true;
		UpdatedAt = DateTime.UtcNow;
	}
}