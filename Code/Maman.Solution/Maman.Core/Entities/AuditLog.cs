using Maman.Core.Common;

namespace Maman.Core.Entities;

public class AuditLog : BaseEntity
{
	public string AdminUserId { get; set; } = string.Empty;
	public string IpAddress { get; set; } = string.Empty;
	public string Action { get; set; } = string.Empty;
	public AuditTarget Target { get; set; } = new();
	public AuditChanges? Changes { get; set; }
	public string? Reason { get; set; }
	public string? RequestId { get; set; }
}

public class AuditTarget
{
	public string Collection { get; set; } = string.Empty; // table
	public string DocumentId { get; set; } = string.Empty; // row
}

public class AuditChanges
{
	public string Field { get; set; } = string.Empty;
	public object? OldValue { get; set; }
	public object? NewValue { get; set; }
}

