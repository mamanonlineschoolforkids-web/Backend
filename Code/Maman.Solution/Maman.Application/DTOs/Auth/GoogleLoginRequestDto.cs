using Maman.Core.Enums;

namespace Maman.Application.DTOs.Auth;

public class GoogleLoginRequestDto
{
	public string GoogleId { get; set; }
	public string IdToken { get; set; }
	public string Country { get; set; }
	public string PhoneNumber { get; set; }
	public UserRole Role { get; set; }

}