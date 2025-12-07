namespace Maman.Core.Common;

public abstract class AuditableEntity : BaseEntity
{
	public string? CreatedBy { get; set; } = "Self";
	public string? UpdatedBy { get; set; } = "Self";
	public string? DeletedBy { get; set; } = "Self";
}