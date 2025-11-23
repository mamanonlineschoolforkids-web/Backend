using Maman.Core.Common;
using Maman.Core.Interfaces;
using Maman.Core.Specifications;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Maman.Infrastructure.Persistence.Repositories;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
	protected readonly IMongoCollection<T> _collection;

	public MongoRepository(IMongoCollection<T> collection)
	{
		_collection = collection;
	}

	public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _collection
			.Find(x => x.Id == id && !x.IsDeleted)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _collection
			.Find(x => !x.IsDeleted)
			.ToListAsync(cancellationToken);
	}
	public virtual async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
	{
		var query = ApplySpecification(specification);
		return await query.ToListAsync(cancellationToken);
	}

	public virtual async Task<T?> FindOneAsync(
		ISpecification<T> specification,
		CancellationToken cancellationToken = default)
	{
		var query = ApplySpecification(specification);
		return await query.FirstOrDefaultAsync(cancellationToken);
	}

	public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
	{
		entity.CreatedAt = DateTime.UtcNow;
		entity.UpdatedAt = DateTime.UtcNow;
		await _collection.InsertOneAsync(entity, null, cancellationToken);
		return entity;
	}

	public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
	{
		entity.UpdatedAt = DateTime.UtcNow;
		await _collection.ReplaceOneAsync(
			x => x.Id == entity.Id,
			entity,
			new ReplaceOptions { IsUpsert = false },
			cancellationToken);
	}

	public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		var filter = Builders<T>.Filter.Eq("_id", id);
		await _collection.DeleteOneAsync(filter, cancellationToken);
	}

	public virtual async Task<long> CountAsync(
		Expression<Func<T, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
	{
		if (predicate == null)
			return await _collection.CountDocumentsAsync(x => !x.IsDeleted, null, cancellationToken);

		return await _collection.CountDocumentsAsync(predicate, null, cancellationToken);
	}


	public virtual async Task<(IEnumerable<T> Items, long TotalCount)> GetPagedAsync(
		ISpecification<T> specification,
		CancellationToken cancellationToken = default)
	{
		var totalCount = await CountAsync(specification.Criteria);

		var items = await FindAsync(specification,cancellationToken);

		return (items, totalCount);
	}

	protected IFindFluent<T, T> ApplySpecification(ISpecification<T> specification)
    {
        var query = specification.Criteria != null 
            ? _collection.Find(specification.Criteria) 
            : _collection.Find(T => !T.IsDeleted);

        if (specification.OrderBy != null)
        {
            query = query.SortBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.SortByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Limit(specification.Take);
        }

        return query;
    }

}
