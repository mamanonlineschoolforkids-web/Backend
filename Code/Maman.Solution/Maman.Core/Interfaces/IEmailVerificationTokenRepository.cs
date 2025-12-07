using Maman.Core.Entities;
using Maman.Core.Entities.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Core.Interfaces;

public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>
{
	//Task<EmailVerificationToken> GetValidTokenAsync(string token, CancellationToken cancellationToken);
	Task InvalidateAllUserTokensAsync(string id, CancellationToken cancellationToken);
}
