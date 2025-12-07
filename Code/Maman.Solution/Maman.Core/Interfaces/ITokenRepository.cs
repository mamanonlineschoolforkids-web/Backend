using Maman.Core.Entities.Tokens;
using Maman.Core.Enums;

namespace Maman.Core.Interfaces;

public interface ITokenRepository : IRepository<Token>
{
	Task InvalidateAllUserTokensAsync(string id,TokenType tokenType , CancellationToken cancellationToken);
	Task RevokeAllUserTokensAsync(string userId,string? ipAddress = null, CancellationToken cancellationToken = default);
	Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
	Task<int> CleanupInvalidTokensAsync(CancellationToken cancellationToken = default);
	Task<int> CleanupInvalidRefreshTokensAsync(CancellationToken cancellationToken = default);
}
