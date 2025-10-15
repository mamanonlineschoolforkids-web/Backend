namespace Maman.Application.Services;

public class CacheService : ICacheService
{
	private IDatabase _database;
	public CacheService(IConnectionMultiplexer redis)
	{
		_database  = redis.GetDatabase();
	}
	public async Task CacheResponseAsync(string key, object response, TimeSpan timeOut)
	{
		if (response is null) return;


		var serializedResponse = JsonSerializer.Serialize(response);

		await _database.StringSetAsync(key, serializedResponse, timeOut);
	}

	public async Task<string?> GetCachedResponseAsync(string key)
	{
		var cashedResponse = await _database.StringGetAsync(key);

		if (cashedResponse.IsNullOrEmpty) return null;

		return cashedResponse;
	}
}
