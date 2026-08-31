using Microsoft.Extensions.DependencyInjection;

namespace EImece.Domain.Configuration
{
    public static class EimeceMemoryCacheRegistration
    {
        /// <summary>
        /// Registers the shared <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/> used by
        /// <see cref="EImece.Domain.Caching.LazyCacheProvider"/>. Does not set
        /// <see cref="Microsoft.Extensions.Caching.Memory.MemoryCacheOptions.SizeLimit"/> — LazyCache's
        /// internal provider does not assign entry sizes, which would throw at runtime when SizeLimit is set.
        /// <see cref="CacheOptions.SizeLimit"/> remains available for configuration/documentation but is not applied here.
        /// </summary>
        public static IServiceCollection AddEimeceMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            return services;
        }
    }
}
