using Maman.Core.Entities;
using MongoDB.Bson;

namespace Maman.Infrastructure.Persistence;

public class MongoDbContext
{
	private readonly IMongoDatabase _database;

	public MongoDbContext(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);

		_database = client.GetDatabase(settings.Value.DatabaseName);
	}

	//public IMongoCollection<User> Users => _database.GetCollection<User>("users");

	// For Unit of Work pattern with transactions
	public IMongoClient Client { get; }
	public IMongoCollection<T> GetCollection<T>() where T : BaseEntity
	{
		return _database.GetCollection<T>(typeof(T).Name);
	}
}

public class MongoDbSettings
{
	public string ConnectionString { get; set; }
	public string DatabaseName { get; set; }
}