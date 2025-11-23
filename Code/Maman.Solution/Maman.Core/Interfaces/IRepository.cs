using Maman.Core.Specifications;
using System.Linq.Expressions;

namespace Maman.Core.Interfaces;

public interface IRepository<T> where T : class
{
	Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
	Task<T?> FindOneAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
	Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
	Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, CancellationToken cancellationToken = default);
	Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
	Task<(IEnumerable<T> Items, long TotalCount)> GetPagedAsync(ISpecification<T> specification ,CancellationToken cancellationToken = default);
}

