using Maman.Core.Entities.Finance;
using Maman.Core.Interfaces;
using MongoDB.Driver;



namespace Maman.Infrastructure.Persistence.Repositories;


public class FinanceAccountRepository : MongoRepository<FinanceAccount>, IFinanceAccountRepository
{
	public FinanceAccountRepository(MongoDbContext context) : base(context.FinanceAccounts)
	{
	}

	public async Task<FinanceAccount?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
	{
		return await _collection
			.Find(fa => fa.UserId == userId && !fa.IsDeleted)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task UpdateBalanceAsync(string userId, decimal amount, CancellationToken cancellationToken = default)
	{
		var update = Builders<FinanceAccount>.Update
			.Inc(fa => fa.CurrentBalance, amount)
			.Set(fa => fa.UpdatedAt, DateTime.UtcNow);

		await _collection.UpdateOneAsync(
			fa => fa.UserId == userId,
			update,
			new UpdateOptions { IsUpsert = false },
			cancellationToken);
	}
}
