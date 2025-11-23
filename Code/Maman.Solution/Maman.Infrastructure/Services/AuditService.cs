using Maman.Application.Interfaces;
using Maman.Core.Entities;
using Maman.Core.Interfaces;
using Microsoft.Extensions.Logging;


namespace Maman.Application.Services.Utility;

public class AuditService : IAuditService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AuditService> _logger;

	public AuditService(IUnitOfWork unitOfWork, ILogger<AuditService> logger)
	{
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task LogAdminActionAsync(
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
		CancellationToken cancellationToken = default)
	{
		try
		{
			var auditLog = new AuditLog
			{
				AdminUserId = adminUserId,
				Action = action,
				IpAddress = ipAddress ?? "Unknown",
				Target = new AuditTarget
				{
					Collection = collection,
					DocumentId = documentId
				},
				Changes = field != null ? new AuditChanges
				{
					Field = field,
					OldValue = oldValue,
					NewValue = newValue
				} : null,
				Reason = reason,
				RequestId = requestId
			};

			await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
			_logger.LogInformation("Admin action logged: {Action} by {AdminUserId} on {Collection}/{DocumentId}",
				action, adminUserId, collection, documentId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to log admin action");
			// Don't throw - auditing failure shouldn't break the main operation
		}
	}
}

