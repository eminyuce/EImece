namespace EImece.Domain.Core.Caching;

/// <summary>
/// Cache abstraction (parity with legacy EImece.Domain.Caching.IEimeceCacheProvider).
/// Duration parameters are in seconds.
/// </summary>
public interface IEimeceCacheProvider
{
    bool Get<T>(string key, out T? value);
    T GetOrAdd<T>(string key, Func<T> valueFactory, int durationSeconds);
    Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, int durationSeconds);
    void Set<T>(string key, T value, int durationSeconds);
    void Clear(string key);
    void ClearAll();
}
