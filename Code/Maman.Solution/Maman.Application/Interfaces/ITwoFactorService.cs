using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Interfaces;

public interface ITwoFactorService
{
	string GenerateSecret();
	string GenerateQrCodeUrl(string email, string secret, string issuer);
	bool ValidateCode(string secret, string code);
	string GetManualEntryKey(string secret);
}
