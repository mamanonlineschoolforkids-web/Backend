using Maman.Core;
using Maman.Core.Entities;
using Maman.Core.Interfaces.Repositories;
using Maman.Infrastructure.Data;
using Maman.Infrastructure.Repositories;
using System.Collections;

namespace Maman.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
	private readonly MongoDbContext _context;
	private Hashtable _repositories;

	public UnitOfWork(MongoDbContext context)
	{
		_context = context;
		_repositories = new Hashtable();
	}

	public IGenericRepository<T> Repository<T>() where T : BaseEntity
	{
		var key = typeof(T).Name;

		if (!_repositories.ContainsKey(key))
		{
			var repository = new GenericRepository<T>(_context);

			_repositories.Add(key, repository);
		}

		return _repositories[key] as IGenericRepository<T>;
	}

	public async Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> action)
	{
		 using var session = await _context.Client.StartSessionAsync();

		session.StartTransaction();

		try
		{
			await action(session);

			await session.CommitTransactionAsync();
		}
		catch (Exception)
		{
			await session.AbortTransactionAsync();
			throw;
		}
	}


	public ValueTask DisposeAsync()
	{
		_repositories.Clear();

		return ValueTask.CompletedTask;
	}
}