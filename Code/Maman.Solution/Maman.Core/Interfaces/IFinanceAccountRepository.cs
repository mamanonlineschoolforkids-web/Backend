using Maman.Core.Entities.Finance;

namespace Maman.Core.Interfaces;

public interface IFinanceAccountRepository : IRepository<FinanceAccount>
{
	Task<FinanceAccount?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
	Task UpdateBalanceAsync(string userId, decimal amount, CancellationToken cancellationToken = default);
}