using System;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public interface IEimeceCacheProvider
    {
        /// <summary>
        /// Retrieve cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Name of cached item</param>
        /// <param name="value">Cached value. Default(T) if
        /// item doesn't exist.</param>
        /// <returns>Cached item as type</returns>
        bool Get<T>(string key, out T value);

        /// <summary>
        /// Atomically returns the cached item for <paramref name="key"/>, or invokes
        /// <paramref name="valueFactory"/> exactly once to populate it. Concurrent callers that
        /// miss the same key are coalesced onto a single factory execution ("single-flight"),
        /// eliminating the cache-stampede / thundering-herd that the classic get-then-set pattern
        /// suffers on expiry of a hot key.
        /// </summary>
        /// <typeparam name="T">Type of cached item.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="valueFactory">Factory that produces the value on a miss.</param>
        /// <param name="duration">Absolute expiration in seconds.</param>
        T GetOrAdd<T>(string key, Func<T> valueFactory, int duration);

        /// <summary>
        /// Single-flight get-or-add with an explicit <see cref="CachePolicy"/> (absolute or sliding).
        /// </summary>
        T GetOrAdd<T>(string key, Func<T> valueFactory, CachePolicy policy);

        /// <summary>
        /// Asynchronous, single-flight counterpart of <see cref="GetOrAdd{T}(string, Func{T}, int)"/>.
        /// The awaited factory runs at most once per key even under concurrent misses, so an
        /// expensive async I/O population (HTTP, DB) is never fanned out across every waiting request.
        /// </summary>
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, int duration);

        /// <summary>
        /// Asynchronous single-flight get-or-add with an explicit <see cref="CachePolicy"/>.
        /// </summary>
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, CachePolicy policy);

        /// <summary>
        /// Insert value into the cache using
        /// appropriate name/value pairs WITH a cache duration set in seconds (absolute).
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Item to be cached</param>
        /// <param name="value">Name of item</param>
        /// <param name="duration">Cache duration in seconds</param>
        void Set<T>(string key, T value, int duration);

        /// <summary>
        /// Insert value using an explicit absolute or sliding <see cref="CachePolicy"/>.
        /// </summary>
        void Set<T>(string key, T value, CachePolicy policy);

        /// <summary>
        /// Remove item from cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        void Clear(string key);

        /// <summary>
        /// Removes every entry whose logical key starts with <paramref name="keyPrefix"/>.
        /// Use hierarchical keys from <see cref="CacheKeys"/> so a save/delete can drop a whole
        /// product-list family without clearing unrelated settings or order caches.
        /// </summary>
        int ClearByPrefix(string keyPrefix);

        void ClearAll();
    }
}
