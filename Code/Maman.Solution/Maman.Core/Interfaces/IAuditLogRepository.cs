using Maman.Core.Entities;

namespace Maman.Core.Interfaces;
public interface IAuditLogRepository : IRepository<AuditLog>
{
	Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<AuditLog>> GetByActionAsync(string action, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}