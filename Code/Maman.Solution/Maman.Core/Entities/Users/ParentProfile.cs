namespace Maman.Core.Entities.Auth;

public class ParentProfile
{
	public int Points { get; set; } = 0;
	public List<string> LinkedStudentIds { get; set; } = new();
}