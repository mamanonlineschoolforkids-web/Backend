using Maman.Core.Entities;
using Maman.Core.Entities.Auth;
using Maman.Core.Entities.Finance;
using Maman.Core.Entities.Tokens;
using Maman.Core.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence;

public class MongoDbContext
{
	private readonly IMongoDatabase _database;
	private readonly MongoDbSettings _settings;
	private IClientSessionHandle? _session;

	public MongoDbContext(IOptions<MongoDbSettings> settings)
	{
		_settings = settings.Value;
		var client = new MongoClient(_settings.ConnectionString);
		_database = client.GetDatabase(_settings.DatabaseName);

	}

	public IMongoCollection<User> Users =>
		_database.GetCollection<User>(_settings.Collections.Users);

	public IMongoCollection<AuditLog> AuditLogs =>
		_database.GetCollection<AuditLog>(_settings.Collections.AuditLogs);

	public IMongoCollection<FinanceAccount> FinanceAccounts =>
		_database.GetCollection<FinanceAccount>(_settings.Collections.FinanceAccounts);

	public IMongoCollection<Token> Tokens =>
	_database.GetCollection<Token>(_settings.Collections.Tokens);

	public async Task<IClientSessionHandle> StartSessionAsync()
	{
		_session = await _database.Client.StartSessionAsync();
		return _session;
	}

	public IClientSessionHandle? Session => _session;

}


