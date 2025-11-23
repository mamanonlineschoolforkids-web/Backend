using Maman.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Maman.Application.Services.Utility;

public class HybridCacheService : ICacheService
{
	private readonly IMemoryCache _l1Cache; // In-memory cache
	private readonly IDistributedCache _l2Cache; // Redis cache
	private readonly ILogger<HybridCacheService> _logger;
	private readonly TimeSpan _l1DefaultExpiration = TimeSpan.FromMinutes(5);
	private readonly TimeSpan _l2DefaultExpiration = TimeSpan.FromHours(1);

	public HybridCacheService(
		IMemoryCache memoryCache,
		IDistributedCache distributedCache,
		ILogger<HybridCacheService> logger)
	{
		_l1Cache = memoryCache;
		_l2Cache = distributedCache;
		_logger = logger;
	}

	public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
	{
		// Try L1 cache first
		if (_l1Cache.TryGetValue(key, out T? l1Value))
		{
			_logger.LogDebug("Cache hit (L1): {Key}", key);
			return l1Value;
		}

		// Try L2 cache
		try
		{
			var l2Value = await _l2Cache.GetStringAsync(key, cancellationToken);
			if (!string.IsNullOrEmpty(l2Value))
			{
				_logger.LogDebug("Cache hit (L2): {Key}", key);
				var deserializedValue = JsonSerializer.Deserialize<T>(l2Value);

				// Populate L1 cache
				_l1Cache.Set(key, deserializedValue, _l1DefaultExpiration);

				return deserializedValue;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading from L2 cache for key: {Key}", key);
		}

		_logger.LogDebug("Cache miss: {Key}", key);
		return default;
	}

	public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
	{
		var l1Expiration = expiration ?? _l1DefaultExpiration;
		var l2Expiration = expiration ?? _l2DefaultExpiration;

		// Set in L1 cache
		_l1Cache.Set(key, value, l1Expiration);

		// Set in L2 cache
		try
		{
			var serializedValue = JsonSerializer.Serialize(value);
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = l2Expiration
			};

			await _l2Cache.SetStringAsync(key, serializedValue, options, cancellationToken);
			_logger.LogDebug("Cache set: {Key}", key);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error writing to L2 cache for key: {Key}", key);
		}
	}

	public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
	{
		// Remove from L1
		_l1Cache.Remove(key);

		// Remove from L2
		try
		{
			await _l2Cache.RemoveAsync(key, cancellationToken);
			_logger.LogDebug("Cache removed: {Key}", key);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing from L2 cache for key: {Key}", key);
		}
	}

	public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
	{
		// Check L1
		if (_l1Cache.TryGetValue(key, out _))
		{
			return true;
		}

		// Check L2
		try
		{
			var value = await _l2Cache.GetStringAsync(key, cancellationToken);
			return !string.IsNullOrEmpty(value);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking existence in L2 cache for key: {Key}", key);
			return false;
		}
	}

	public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
	{
		// Note: This is a simplified implementation
		// For production, consider using Redis SCAN command with pattern matching
		_logger.LogWarning("RemoveByPrefix is not fully implemented for distributed cache. Prefix: {Prefix}", prefix);
		await Task.CompletedTask;
	}

	public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
	{
		throw new NotImplementedException();
	}
}
