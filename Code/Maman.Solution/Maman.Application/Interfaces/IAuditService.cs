namespace Maman.Application.Interfaces
{
	public interface IAuditService
	{
		Task LogAdminActionAsync(
		string adminUserId,
		string action,
		string collection,
		string documentId,
		string? field = null,
		object? oldValue = null,
		object? newValue = null,
		string? reason = null,
		string? requestId = null,
		string? ipAddress = null,
		CancellationToken cancellationToken = default);
	}
}