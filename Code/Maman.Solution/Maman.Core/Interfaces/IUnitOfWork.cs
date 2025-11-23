namespace Maman.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
	IUserRepository Users { get; }
	IRefreshTokenRepository RefreshTokens { get; }
	IFinanceAccountRepository FinanceAccounts { get; }
	IAuditLogRepository AuditLogs { get; }
	IEmailVerificationTokenRepository EmailVerificationTokens { get; }
	IPasswordResetTokenRepository PasswordResetTokens { get; }



	Task BeginTransactionAsync(CancellationToken cancellationToken = default);
	Task CommitTransactionAsync(CancellationToken cancellationToken = default);
	Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}