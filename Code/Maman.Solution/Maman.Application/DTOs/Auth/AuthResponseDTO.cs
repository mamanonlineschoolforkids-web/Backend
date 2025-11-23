using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.Auth;

public class AuthResponseDto
{
	public string RefreshToken { get; set; }
	public string AccessToken { get; set; }
	public DateTime ExpiresAt { get; set; }
	public UserDto User { get; set; }

}
