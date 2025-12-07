namespace Maman.Core.Entities.Auth;

public class AdminProfile
{
	public bool IsSuperAdmin { get; set; }
	public bool TwoFactorEnabled { get; set; }
	public Dictionary<string, Permissions> Permissions { get; set; } = new();
}

public class Permissions
{
	public bool Read { get; set; }
	public bool Write { get; set; }
	public bool Delete { get; set; }
	public bool Publish { get; set; }
}