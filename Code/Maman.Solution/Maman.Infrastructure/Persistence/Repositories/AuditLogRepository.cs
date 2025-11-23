using Maman.Core.Entities;
using Maman.Core.Interfaces;
using Maman.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence.Repositories;


public class AuditLogRepository : MongoRepository<AuditLog>, IAuditLogRepository
{
	public AuditLogRepository(MongoDbContext context) : base(context.AuditLogs)
	{
	}

	public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(
		string userId,
		int pageNumber = 1,
		int pageSize = 50,
		CancellationToken cancellationToken = default)
	{
		var skip = (pageNumber - 1) * pageSize;

		return await _collection
			.Find(al => al.AdminUserId == userId)
			.SortByDescending(al => al.CreatedAt)
			.Skip(skip)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
		string entityType,
		string entityId,
		CancellationToken cancellationToken = default)
	{
		return await _collection
			.Find(al => al.Target.Collection == entityType && al.Target.DocumentId == entityId)
			.SortByDescending(al => al.CreatedAt)
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(
		string action,
		DateTime? from = null,
		DateTime? to = null,
		CancellationToken cancellationToken = default)
	{
		var filterBuilder = Builders<AuditLog>.Filter;
		var filters = new List<FilterDefinition<AuditLog>>
		{
			filterBuilder.Eq(al => al.Action, action)
		};

		if (from.HasValue)
			filters.Add(filterBuilder.Gte(al => al.CreatedAt, from.Value));

		if (to.HasValue)
			filters.Add(filterBuilder.Lte(al => al.CreatedAt, to.Value));

		var combinedFilter = filterBuilder.And(filters);

		return await _collection
			.Find(combinedFilter)
			.SortByDescending(al => al.CreatedAt)
			.ToListAsync(cancellationToken);
	}

	// Override to prevent updates
	public override async Task UpdateAsync(AuditLog entity, CancellationToken cancellationToken = default)
	{
		throw new InvalidOperationException("Audit logs cannot be updated");
	}

	// Override to prevent deletes
	public override async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		throw new InvalidOperationException("Audit logs cannot be deleted from application layer");
	}
}