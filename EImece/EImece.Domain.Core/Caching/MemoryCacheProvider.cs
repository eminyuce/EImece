using System.Collections.Concurrent;
using EImece.Domain.Core.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Core.Caching;

/// <summary>
/// IMemoryCache-backed provider with single-flight GetOrAdd (cache-stampede protection).
/// </summary>
public sealed class MemoryCacheProvider : IEimeceCacheProvider
{
    private const string KeyPrefix = "Memory:";
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _knownKeys = new(StringComparer.Ordinal);

    public MemoryCacheProvider(IMemoryCache cache, IOptions<CacheOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool Get<T>(string key, out T? value)
    {
        value = default;
        if (!_options.IsCacheActive)
        {
            return false;
        }

        if (_cache.TryGetValue(Normalize(key), out T? cached) && cached is not null)
        {
            value = cached;
            return true;
        }

        return false;
    }

    public T GetOrAdd<T>(string key, Func<T> valueFactory, int durationSeconds)
    {
        if (!_options.IsCacheActive)
        {
            return valueFactory();
        }

        var cacheKey = Normalize(key);
        if (_cache.TryGetValue(cacheKey, out T? existing) && existing is not null)
        {
            return existing;
        }

        var gate = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        gate.Wait();
        try
        {
            if (_cache.TryGetValue(cacheKey, out existing) && existing is not null)
            {
                return existing;
            }

            var created = valueFactory();
            SetCore(cacheKey, created, durationSeconds);
            return created;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, int durationSeconds)
    {
        if (!_options.IsCacheActive)
        {
            return await valueFactory().ConfigureAwait(false);
        }

        var cacheKey = Normalize(key);
        if (_cache.TryGetValue(cacheKey, out T? existing) && existing is not null)
        {
            return existing;
        }

        var gate = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_cache.TryGetValue(cacheKey, out existing) && existing is not null)
            {
                return existing;
            }

            var created = await valueFactory().ConfigureAwait(false);
            SetCore(cacheKey, created, durationSeconds);
            return created;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Set<T>(string key, T value, int durationSeconds)
    {
        if (!_options.IsCacheActive)
        {
            return;
        }

        SetCore(Normalize(key), value, durationSeconds);
    }

    public void Clear(string key)
    {
        var cacheKey = Normalize(key);
        _cache.Remove(cacheKey);
        _knownKeys.TryRemove(cacheKey, out _);
    }

    public void ClearAll()
    {
        foreach (var key in _knownKeys.Keys)
        {
            _cache.Remove(key);
        }

        _knownKeys.Clear();
    }

    private void SetCore<T>(string cacheKey, T value, int durationSeconds)
    {
        var seconds = Math.Max(1, durationSeconds);
        _cache.Set(cacheKey, value, TimeSpan.FromSeconds(seconds));
        _knownKeys[cacheKey] = 0;
    }

    private static string Normalize(string key) =>
        key.StartsWith(KeyPrefix, StringComparison.Ordinal) ? key : KeyPrefix + key;
}
