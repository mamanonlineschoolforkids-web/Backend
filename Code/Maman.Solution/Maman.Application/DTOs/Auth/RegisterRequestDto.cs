
using Maman.Core.Enums;

namespace Maman.Application.DTOs.Auth;

public class RegisterRequestDto
{
	public string Name { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public string ConfirmPassword { get; set; }
	public string Country { get; set; }
	public string PhoneNumber { get; set; }
	public UserRole Role { get; set; }
	public string PreferredLanguage { get; set; } = "ar";

}
