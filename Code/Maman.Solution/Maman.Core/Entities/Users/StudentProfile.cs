namespace Maman.Core.Entities.Auth;

public class StudentProfile
{
	public DateTime DateOfBirth { get; set; }
	public string? ParentId { get; set; }
	public int Points { get; set; } = 0;
	public int DailyUsageLimitInMinutes { get; set; } = 1440;
	public int ElapsedTimeOfTheDayInMinutes { get; set; } = 0;
}