namespace Maman.Application.Interfaces;

public interface ICurrentUserService
{
	public string? UserId { get; }
	public string? Email { get; }
	public List<string> Roles { get; }
	public string? IpAddress { get; }
	public bool IsAuthenticated { get;}

}
