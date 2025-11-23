using Maman.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.Auth
{
	public class UserDto
	{
		public string Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;

		public string Country { get; set; } = string.Empty;

		public string PhoneNumber { get; set; } = string.Empty;

		public string GoogleId { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public bool IsEmailVerified { get; set; }

		public UserRole Role { get; set; }

		public DateTime LastLogin { get; set; }

		public string Status { get; set; }
		public string DisplayCalendar { get; set; }
		public string PreferredLanguage { get; set; }
		public bool TwoFactorEnabled { get; set; }

		public DateTime CreatedAt { get; set; }

	}
}
