using EImece.Domain.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EImece.Tests.Infrastructure
{
    internal static class TestNullLoggers
    {
        public static ILoggerFactory Factory { get; } = NullLoggerFactory.Instance;

        public static ILogger Create() => NullLogger.Instance;

        public static ILogger<T> Create<T>() => NullLogger<T>.Instance;

        public static LazyCacheProvider CreateLazyCacheProvider()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            return new LazyCacheProvider(Create<LazyCacheProvider>(), memoryCache);
        }
    }
}
