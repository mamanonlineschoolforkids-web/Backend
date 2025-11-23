using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.Auth
{
	public class TwoFactorSetupResponseDto
	{
		public string Secret { get; set; }
		public string QrCodeUrl { get; set; }
		public string ManualEntryKey { get; set; }


	}
}
