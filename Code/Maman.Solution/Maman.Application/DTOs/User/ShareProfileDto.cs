using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.User
{
	public class ShareProfileDto
	{
		public bool IncludeEmail { get; set; }
		public bool IncludePhoneNumber { get; set; }
		public List<string> IncludeFields { get; set; }

	}
}
