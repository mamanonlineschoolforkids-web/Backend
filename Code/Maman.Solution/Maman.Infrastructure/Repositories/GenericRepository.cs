using Maman.Core.Entities;
using Maman.Core.Interfaces.Repositories;
using Maman.Core.Specifications;
using Maman.Infrastructure.Data;
using Maman.Infrastructure.Specifications;
using SharpCompress.Common;

namespace Maman.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
	protected readonly IMongoCollection<T> _collection;
	public GenericRepository(MongoDbContext context)
	{
		_collection = context.GetCollection<T>();
		//_collection = context.GetCollection<T>(typeof(T).Name);
	}

	public async Task<T> GetWithSpecAsync(ISpecification<T> spec)
	{
		return await ApplySpecification(spec).FirstOrDefaultAsync();
	}

	public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
	{
		return await ApplySpecification(spec).ToListAsync();
	}

	public async Task<T> GetByIdAsync(string id)
	{
		return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
	}

	public async Task<IReadOnlyList<T>> GetAllAsync()
	{
		return await _collection.Find(_ => true).ToListAsync();
	}


	public async Task AddAsync(T entity, IClientSessionHandle? session = null)
	{
		if (session != null)
		{
			await _collection.InsertOneAsync(session, entity);
		}
		else
		{
			await _collection.InsertOneAsync(entity);
		}
	}

	public async Task UpdateAsync(T entity, IClientSessionHandle? session = null)
	{
		var filter = Builders<T>.Filter.Eq(doc => doc.Id, entity.Id);

		if (session != null)
		{
			await _collection.ReplaceOneAsync(session, filter, entity);
		}
		else
		{
			await _collection.ReplaceOneAsync(filter, entity);
		}
	}

	public async Task DeleteAsync(string id, IClientSessionHandle? session = null)
	{
		if (session != null)
		{
			await _collection.DeleteOneAsync(session, x => x.Id == id);
		}
		else
		{
			await _collection.DeleteOneAsync(x => x.Id == id);
		}

	}

	private IFindFluent<T, T> ApplySpecification(ISpecification<T> spec)
	{
		return SpecificationEvaluator<T>.GetQuery(_collection.Find(spec.Criteria), spec);
	}

}
