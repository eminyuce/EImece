using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EImece.Tests.Infrastructure
{
    internal static class TestNullLoggers
    {
        public static ILoggerFactory Factory { get; } = NullLoggerFactory.Instance;

        public static ILogger Create() => NullLogger.Instance;

        public static ILogger<T> Create<T>() => NullLogger<T>.Instance;
    }
}
