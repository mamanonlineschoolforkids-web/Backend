using Maman.Core.Entities;
using Maman.Core.Specifications;
using MongoDB.Driver;

namespace Maman.Core.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
	Task<T> GetByIdAsync(string id);
	Task<IReadOnlyList<T>> GetAllAsync();
	Task<T> GetWithSpecAsync(ISpecification<T> spec);
	Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec);
	Task AddAsync(T entity, IClientSessionHandle? session = null);
	Task UpdateAsync(T entity, IClientSessionHandle? session = null);
	Task DeleteAsync(string id, IClientSessionHandle? session = null);
}
