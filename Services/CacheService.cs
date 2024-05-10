using System.Collections.Concurrent;

public class CacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
    private readonly TimeSpan _expirationPeriod;
    private readonly Timer _cleanupTimer;

    public CacheService(TimeSpan expirationPeriod)
    {
        _expirationPeriod = expirationPeriod;
        _cleanupTimer = new Timer(Cleanup, null, expirationPeriod, expirationPeriod);
    }

    public void Add(string key, object value)
    {
        _cache[key] = new CacheItem { Value = value, LastAccessed = DateTime.UtcNow };
    }

    public object Get(string key)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            item.LastAccessed = DateTime.UtcNow;
            return item.Value;
        }

        return null;
    }

    private void Cleanup(object state)
    {
        var keysToRemove = _cache.Where(pair => DateTime.UtcNow - pair.Value.LastAccessed > _expirationPeriod)
                                 .Select(pair => pair.Key)
                                 .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private class CacheItem
    {
        public object Value { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}
