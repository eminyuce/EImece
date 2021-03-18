using System.Collections.Generic;

namespace EImece.Domain.Caching
{
    public abstract class CacheProvider<TCache> : ICacheProvider
    {
        public int CacheDuration
        {
            get;
            set;
        }

        public bool IsCacheProviderActive
        {
            get; set;
        }


        protected TCache _cache;

        public CacheProvider()
        {
            _cache = InitCache();
        }

        public CacheProvider(int durationInMinutes)
        {
            CacheDuration = durationInMinutes;
            _cache = InitCache();
        }

        protected abstract TCache InitCache();

        public abstract bool Get<T>(string key, out T value);

        public abstract void Set<T>(string key, T value);

        public abstract void Set<T>(string key, T value, int duration);

        public abstract void Clear(string key);

        public abstract void ClearAll();

        public abstract IEnumerable<KeyValuePair<string, object>> GetAll();
    }
}