namespace EImece.Domain.Configuration
{
    /// <summary>
    /// In-memory cache settings for storefront read models (settings, menus, categories, etc.).
    /// </summary>
    public sealed class CacheOptions
    {
        public bool IsActive { get; set; } = true;

        public int LongSeconds { get; set; } = 900;

        public int VeryLongSeconds { get; set; } = 86400;

        /// <summary>
        /// Optional bounded size read from Web.config (<c>Cache:SizeLimit</c>).
        /// Not applied to the LazyCache-backed <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>
        /// instance because LazyCache does not set per-entry sizes (would throw when SizeLimit is enabled).
        /// </summary>
        public int SizeLimit { get; set; } = 10_000;

        public static CacheOptions FromAppConfig()
        {
            return Cached.Value;
        }

        internal static void ResetForTests()
        {
            Cached = new System.Lazy<CacheOptions>(BuildFromAppConfig);
        }

        private static System.Lazy<CacheOptions> Cached = new System.Lazy<CacheOptions>(BuildFromAppConfig);

        private static CacheOptions BuildFromAppConfig()
        {
            return new CacheOptions
            {
                IsActive = AppConfig.IsCacheActive,
                LongSeconds = AppConfig.CacheLongSeconds,
                VeryLongSeconds = AppConfig.CacheVeryLongSeconds,
                SizeLimit = AppConfig.GetConfigInt("Cache:SizeLimit", 10_000),
            };
        }
    }
}
