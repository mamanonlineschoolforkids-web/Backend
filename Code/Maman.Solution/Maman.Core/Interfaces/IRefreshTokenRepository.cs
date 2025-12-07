using Maman.Core.Entities.Tokens;
namespace Maman.Core.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
	Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken cancellationToken = default);
	Task RevokeAllUserTokensAsync(string userId, string? ipAddress = null, CancellationToken cancellationToken = default);
	Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
	Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
}
