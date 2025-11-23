using Maman.Core.Entities.Auth;
using Maman.Core.Entities.Tokens;
using Maman.Core.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Infrastructure.Persistence.Repositories;

public class EmailVerificationTokenRepository : MongoRepository<EmailVerificationToken>, IEmailVerificationTokenRepository
{
	public EmailVerificationTokenRepository(MongoDbContext context) : base(context.EmailVerificationTokens)
	{
	}

	public async Task InvalidateAllUserTokensAsync(string userId, CancellationToken cancellationToken)
	{
		var userFilter = Builders<EmailVerificationToken>.Filter.Eq(t => t.UserId, userId);

		var notUsedFilter = Builders<EmailVerificationToken>.Filter.Eq(t => t.IsUsed, false);

		// 3. Filter for tokens that are NOT yet expired (Optional, but good practice to only update relevant ones)
		var notExpiredFilter = Builders<EmailVerificationToken>.Filter.Gt(t => t.ExpiresAt, DateTime.UtcNow);

		var filter = Builders<EmailVerificationToken>.Filter.And(
			userFilter,
			notUsedFilter,
			notExpiredFilter
		);

		var update = Builders<EmailVerificationToken>.Update
			.Set(t => t.IsUsed, true) 
			.Set(t => t.UsedAt, DateTime.UtcNow); 

		// Perform a multi-document update
		await _collection.UpdateManyAsync(
			filter,
			update,
			cancellationToken: cancellationToken
		);
	}
}