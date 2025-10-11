using Maman.Core.Entities;
using Maman.Core.Interfaces.Repositories;
using MongoDB.Driver;

namespace Maman.Core;

public interface IUnitOfWork : IAsyncDisposable
{
	IGenericRepository<T> Repository<T>() where T : BaseEntity;

	Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> action);
}