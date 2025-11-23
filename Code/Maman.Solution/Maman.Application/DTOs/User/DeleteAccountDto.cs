using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.User
{
	public class DeleteAccountDto
	{
		public string Password { get; set; }
		public string Reason { get; set; }
	}
}
