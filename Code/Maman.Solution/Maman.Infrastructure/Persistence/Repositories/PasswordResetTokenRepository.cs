using Maman.Core.Entities.Tokens;
using Maman.Core.Interfaces;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository : MongoRepository<PasswordResetToken> , IPasswordResetTokenRepository
{
	public PasswordResetTokenRepository(MongoDbContext context) : base(context.PasswordResetTokens)
	{
	}

	public async Task InvalidateAllUserTokensAsync(string userId, CancellationToken cancellationToken)
	{
		var userFilter = Builders<PasswordResetToken>.Filter.Eq(t => t.UserId, userId);

		// 2. Filter for tokens that are NOT yet used (IsUsed == false)
		var notUsedFilter = Builders<PasswordResetToken>.Filter.Eq(t => t.IsUsed, false);

		// 3. Filter for tokens that are NOT yet expired (Optional, but good practice to only update relevant ones)
		var notExpiredFilter = Builders<PasswordResetToken>.Filter.Gt(t => t.ExpiresAt, DateTime.UtcNow);

		// Combine all criteria
		var filter = Builders<PasswordResetToken>.Filter.And(
			userFilter,
			notUsedFilter,
			notExpiredFilter
		);

		// Define the update operation:
		var update = Builders<PasswordResetToken>.Update
			.Set(t => t.IsUsed, true) // Set IsUsed to true
			.Set(t => t.UsedAt, DateTime.UtcNow); // Record the time of invalidation

		// Perform a multi-document update
		await _collection.UpdateManyAsync(
			filter,
			update,
			cancellationToken: cancellationToken
		);
	}

	
}
