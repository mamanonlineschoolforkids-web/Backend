namespace Maman.Core.Settings;

public class MongoDbSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;
	public CollectionSettings Collections { get; set; } = new();
}

public class CollectionSettings
{
	public string Users { get; set; } = "users";
	public string RefreshTokens { get; set; } = "refreshTokens";
	public string FinanceAccounts { get; set; } = "financeAccounts";
	public string AuditLogs { get; set; } = "auditLogs";
	public string Tokens { get; set; } = "tokens";
}
