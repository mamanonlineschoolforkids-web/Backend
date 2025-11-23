using Maman.Core.Common;
using Maman.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Maman.Core.Entities.Finance;

public class FinanceAccount : BaseEntity
{
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = string.Empty;

	public PaymentDetails PaymentDetails { get; set; } = new();
	public string PreferredPayoutMethod { get; set; } = string.Empty;
	public PayoutSchedule PayoutSchedule { get; set; }
	public decimal CurrentBalance { get; set; }
	public decimal MinimumPayoutThreshold { get; set; } = 10.0m;
	public List<RevenueShare> RevenueShares { get; set; } = new();
	public DateTime? LastAccessAt { get; set; }
	public DateTime? LastPayoutAt { get; set; }
}

public class PaymentDetails
{
	public string? PaypalEmailEncrypted { get; set; }
	public BankAccountDetails? BankAccount { get; set; }
}

public class BankAccountDetails
{
	public string? AccountNumberEncrypted { get; set; }
	public string? RoutingNumberEncrypted { get; set; }
	public string? BankName { get; set; }
	public string? AccountHolderName { get; set; }
}

public class RevenueShare
{
	public string Type { get; set; } = string.Empty; // "courseSale", "serviceSale"
	public decimal Percent { get; set; }
}

