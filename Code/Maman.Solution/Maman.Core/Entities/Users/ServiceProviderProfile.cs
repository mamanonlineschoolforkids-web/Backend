using Maman.Core.Enums;
using MongoDB.Bson;

namespace Maman.Core.Entities.Auth;

public class ServiceProviderProfile
{
	public string DisplayName { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string Bio { get; set; } = string.Empty;
	public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
	public List<string> SpokenLanguages { get; set; } = new();
	public List<string> ExpertiseTags { get; set; } = new();
	public SocialLinks? SocialLinks { get; set; }
	public List<Experience> Experience { get; set; } = new();
	public List<ServiceOffered> ServicesOffered { get; set; } = new();
	public List<Review> Reviews { get; set; } = new();
	public double AverageRating { get; set; }
	public int TotalStudents { get; set; }
	public int TotalConsultations { get; set; }
}

public class SocialLinks
{
	public string? Youtube { get; set; }
	public string? Facebook { get; set; }
	public string? Website { get; set; }
}

public class Experience
{
	public string Title { get; set; } = string.Empty;
	public string Institution { get; set; } = string.Empty;
	public string Year { get; set; } = string.Empty;
}

public class ServiceOffered
{
	public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public int DurationMinutes { get; set; }
	public decimal Price { get; set; }
	public ServiceRating Rating { get; set; } = new();
}

public class ServiceRating
{
	public int ReviewCount { get; set; }
	public double AverageRating { get; set; }
}

public class Review
{
	public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
	public ReviewUser User { get; set; } = new();
	public int Rating { get; set; }
	public string Comment { get; set; } = string.Empty;
	public string AppointmentId { get; set; }
	public string Status { get; set; } = "Approved";
	public bool IsEdited { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ReviewUser
{
	public string UserId { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
}
