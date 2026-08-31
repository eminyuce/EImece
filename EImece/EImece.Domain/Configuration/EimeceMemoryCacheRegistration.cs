using EImece.Domain.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Configuration
{
    public static class EimeceMemoryCacheRegistration
    {
        public static IServiceCollection AddEimeceMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache(options =>
            {
                var cacheOptions = CacheOptions.FromAppConfig();
                if (cacheOptions.SizeLimit > 0)
                {
                    options.SizeLimit = cacheOptions.SizeLimit;
                }
            });

            return services;
        }
    }
}
