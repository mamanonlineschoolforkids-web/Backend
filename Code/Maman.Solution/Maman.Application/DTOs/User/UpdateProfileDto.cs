using Maman.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.User
{
	public class UpdateProfileDto
	{
		public string Name { get; set; }
		public string PhoneNumber { get; set; }
		public CalendarType DisplayCalendar { get; set; }
		public string Country { get; set; }
		public string PreferredLanguage { get; set; }


	}
}
