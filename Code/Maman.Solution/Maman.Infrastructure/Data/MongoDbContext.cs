using Maman.Core.Entities;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Maman.Infrastructure.Data;

public class MongoDbContext : IDisposable
{
	private readonly IMongoDatabase _database;
	public IMongoClient Client { get; }

	public MongoDbContext(IOptions<MongoDbSettings> settings)
	{

		Client = new MongoClient(settings.Value.ConnectionString);

		_database = Client.GetDatabase(settings.Value.DatabaseName);
	}

	//public IMongoCollection<User> Users => _database.GetCollection<User>("users");

	public IMongoCollection<T> GetCollection<T>() where T : BaseEntity
	{
		return _database.GetCollection<T>(typeof(T).Name);
	}

	public void Dispose()
	{
		Client?.Dispose();
	}

}


public class MongoDbSettings
{
	[Required]
	public string ConnectionString { get; set; }
	[Required]
	public string DatabaseName { get; set; }
}