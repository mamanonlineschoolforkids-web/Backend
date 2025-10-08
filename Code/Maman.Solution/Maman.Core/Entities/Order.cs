namespace Maman.Core.Entities;

public class Order : BaseEntity
{
	public string ProductId { get; set; }
	public int Quantity { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
