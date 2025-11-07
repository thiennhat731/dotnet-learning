using CollabDoc.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CollabDoc.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _cacheKeys;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
        _cacheKeys = new HashSet<string>();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            if (cachedValue is string json)
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            return cachedValue as T;
        }
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = TimeSpan.FromMinutes(5) // Gia hạn cache nếu được truy cập trong 5 phút
        };

        var json = JsonSerializer.Serialize(value);
        _cache.Set(key, json, options);

        lock (_cacheKeys)
        {
            _cacheKeys.Add(key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        lock (_cacheKeys)
        {
            _cacheKeys.Remove(key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var regex = new Regex(pattern.Replace("*", ".*"));
        var keysToRemove = new List<string>();

        lock (_cacheKeys)
        {
            keysToRemove.AddRange(_cacheKeys.Where(key => regex.IsMatch(key)));
        }

        foreach (var key in keysToRemove)
        {
            await RemoveAsync(key);
        }
    }
}
