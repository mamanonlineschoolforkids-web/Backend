using Maman.Core.Entities.Tokens;
using Maman.Core.Enums;
using Maman.Core.Interfaces;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence.Repositories;

public class TokenRepository : MongoRepository<Token>, ITokenRepository
{
	public TokenRepository(MongoDbContext context) : base(context.Tokens)
	{
	}

	public async Task InvalidateAllUserTokensAsync(string userId,TokenType tokenType , CancellationToken cancellationToken)
	{
		var userFilter = Builders<Token>.Filter.Eq(t => t.UserId, userId);

		var notUsedFilter = Builders<Token>.Filter.Eq(t => t.IsUsed, false);

		var notExpiredFilter = Builders<Token>.Filter.Gt(t => t.ExpiresAt, DateTime.UtcNow);

		var typeFilter = Builders<Token>.Filter.Eq(t => t.TokenType,tokenType);


		var filter = Builders<Token>.Filter.And(
			userFilter,
			notUsedFilter,
			notExpiredFilter,
			typeFilter
		);

		var update = Builders<Token>.Update
			.Set(t => t.IsUsed, true)
			.Set(t => t.IsDeleted, true)
			.Set(t => t.DeletedAt, DateTime.UtcNow)
			.Set(t => t.UsedAt, DateTime.UtcNow); 

		await _collection.UpdateManyAsync(
			filter,
			update,
			cancellationToken: cancellationToken
		);
	}

	public async Task RevokeAllUserTokensAsync(
	string userId,
	string? ipAddress = null,
	CancellationToken cancellationToken = default)
	{
		var update = Builders<Token>.Update
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
		var update = Builders<Token>.Update
			.Set(rt => rt.IsRevoked, true)
			.Set(rt => rt.RevokedDate, DateTime.UtcNow);

		await _collection.UpdateOneAsync(
			rt => rt.UserToken == token,
			update,
			cancellationToken: cancellationToken
		);
	}

	//TODO:: Schedule deletion every day
	public async Task<int> CleanupInvalidTokensAsync(CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteManyAsync(
			rt => (rt.ExpiresAt < DateTime.UtcNow || rt.IsDeleted) && rt.TokenType != TokenType.Refresh,
			cancellationToken
		);

		return (int)result.DeletedCount;
	}

	//TODO:: Schedule deletion every week
	public async Task<int> CleanupInvalidRefreshTokensAsync(CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteManyAsync(
			rt => (rt.ExpiresAt < DateTime.UtcNow || rt.IsDeleted) && rt.TokenType == TokenType.Refresh,
			cancellationToken
		);

		return (int)result.DeletedCount;
	}
}