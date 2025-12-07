using Maman.Core.Interfaces;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
	private readonly MongoDbContext _context;
	private IClientSessionHandle? _session;
	private bool _disposed;

	public UnitOfWork(
		MongoDbContext context,
		IUserRepository users,
		IRefreshTokenRepository refreshTokens,
		IFinanceAccountRepository financeAccounts,
		IAuditLogRepository auditLogs ,
		IEmailVerificationTokenRepository emailVerificationTokens,
		IPasswordResetTokenRepository passwordResetTokens
		)
	{
		_context = context;
		Users = users;
		RefreshTokens = refreshTokens;
		FinanceAccounts = financeAccounts;
		AuditLogs = auditLogs;
		EmailVerificationTokens = emailVerificationTokens;
		PasswordResetTokens = passwordResetTokens;
	}

	public IUserRepository Users { get; }
	public IRefreshTokenRepository RefreshTokens { get; }
	public IFinanceAccountRepository FinanceAccounts { get; }
	public IAuditLogRepository AuditLogs { get; }
	public IEmailVerificationTokenRepository EmailVerificationTokens { get; }
	public IPasswordResetTokenRepository PasswordResetTokens { get; }


	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		_session = await _context.Users.Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
		_session.StartTransaction();
	}

	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_session != null)
		{
			await _session.CommitTransactionAsync(cancellationToken);
			_session.Dispose();
			_session = null;
		}
	}

	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_session != null)
		{
			await _session.AbortTransactionAsync(cancellationToken);
			_session.Dispose();
			_session = null;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_session?.Dispose();
		}
		_disposed = true;
	}
}