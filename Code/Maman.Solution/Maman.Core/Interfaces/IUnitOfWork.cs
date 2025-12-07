namespace Maman.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
	IUserRepository Users { get; }
	IFinanceAccountRepository FinanceAccounts { get; }
	IAuditLogRepository AuditLogs { get; }
	ITokenRepository Tokens { get; }

	Task BeginTransactionAsync(CancellationToken cancellationToken = default);
	Task CommitTransactionAsync(CancellationToken cancellationToken = default);
	Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}