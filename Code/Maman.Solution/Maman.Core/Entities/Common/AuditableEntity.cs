namespace Maman.Core.Common;

public abstract class AuditableEntity : BaseEntity
{
	public string? CreatedBy { get; set; }
	public string? UpdatedBy { get; set; }
	public string? DeletedBy { get; set; }
}
