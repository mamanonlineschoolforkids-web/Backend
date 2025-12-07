using Maman.Core.Entities.Tokens;
using Maman.Core.Interfaces;
using Maman.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : MongoRepository<RefreshToken>, IRefreshTokenRepository
{
	public RefreshTokenRepository(MongoDbContext context) : base(context.RefreshTokens)
	{
	}

	public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		return await _collection
			.Find(rt => rt.Token == token && !rt.IsDeleted)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		return await _collection
			.Find(rt => rt.UserId == userId &&
					   !rt.IsRevoked &&
					   !rt.IsUsed &&
					   rt.ExpiresAt > DateTime.UtcNow &&
					   !rt.IsDeleted)
			.ToListAsync(cancellationToken);
	}

	public async Task RevokeAllUserTokensAsync(
		string userId,
		string? ipAddress = null,
		CancellationToken cancellationToken = default)
	{
		var update = Builders<RefreshToken>.Update
			.Set(rt => rt.IsRevoked, true)
			.Set(rt => rt.RevokedDate, DateTime.UtcNow)
			.Set(rt => rt.RevokedByIp, ipAddress)
			.Set(rt => rt.UpdatedAt, DateTime.UtcNow);

		await _collection.UpdateManyAsync(
			rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsDeleted,
			update,
			null,
			cancellationToken);
	}

	public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		var update = Builders<RefreshToken>.Update
			.Set(rt => rt.IsRevoked, true)
			.Set(rt => rt.RevokedDate, DateTime.UtcNow);

		await _collection.UpdateOneAsync(
			rt => rt.Token == token,
			update,
			cancellationToken: cancellationToken
		);
	}

	public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteManyAsync(
			rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked,
			cancellationToken
		);

		return (int)result.DeletedCount;
	}
}