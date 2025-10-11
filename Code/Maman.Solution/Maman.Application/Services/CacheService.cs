using Maman.Core.Interfaces.Services;
using StackExchange.Redis;
using System.Text.Json;

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

		var serializedOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		var serializedResponse = JsonSerializer.Serialize(response, serializedOptions);

		await _database.StringSetAsync(key, serializedResponse, timeOut);
	}

	public async Task<string?> GetCachedResponseAsync(string key)
	{
		var cashedResponse = await _database.StringGetAsync(key);

		if (cashedResponse.IsNullOrEmpty) return null;

		return cashedResponse;
	}
}
