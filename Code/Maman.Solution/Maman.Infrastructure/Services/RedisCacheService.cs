using Maman.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Maman.Application.Services.Utility;

public class RedisCacheService : ICacheService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> _logger;
	private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

	public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
	{
		try
		{
			var cachedValue = await _cache.GetStringAsync(key, cancellationToken);

			if (string.IsNullOrEmpty(cachedValue))
				return null;

			return JsonSerializer.Deserialize<T>(cachedValue);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting cached value for key: {Key}", key);
			return null;
		}
	}

	public async Task SetAsync<T>(
		string key,
		T value,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default) where T : class
	{
		try
		{
			var serializedValue = JsonSerializer.Serialize(value);
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
			};

			await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting cache for key: {Key}", key);
		}
	}

	public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
	{
		try
		{
			await _cache.RemoveAsync(key, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing cache for key: {Key}", key);
		}
	}

	public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
	{
		try
		{
			var value = await _cache.GetStringAsync(key, cancellationToken);
			return !string.IsNullOrEmpty(value);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
			return false;
		}
	}

	public async Task<T> GetOrSetAsync<T>(
		string key,
		Func<Task<T>> factory,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default) where T : class
	{
		var cachedValue = await GetAsync<T>(key, cancellationToken);

		if (cachedValue != null)
			return cachedValue;

		var value = await factory();
		await SetAsync(key, value, expiration, cancellationToken);

		return value;
	}
}