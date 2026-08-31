using Microsoft.Extensions.Logging;
using System;

namespace EImece.Domain.Observability.Logging
{
    /// <summary>
    /// Wraps an <see cref="ILoggerProvider"/> so sink failures never propagate to application code.
    /// </summary>
    internal sealed class FailSafeLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerProvider _inner;

        public FailSafeLoggerProvider(ILoggerProvider inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FailSafeLogger(_inner.CreateLogger(categoryName));
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        private sealed class FailSafeLogger : ILogger
        {
            private readonly ILogger _inner;

            public FailSafeLogger(ILogger inner)
            {
                _inner = inner;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                try
                {
                    return _inner.BeginScope(state);
                }
                catch
                {
                    return NullScope.Instance;
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                try
                {
                    return _inner.IsEnabled(logLevel);
                }
                catch
                {
                    return false;
                }
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                try
                {
                    _inner.Log(logLevel, eventId, state, exception, formatter);
                }
                catch
                {
                    // Fail-safe: provider errors must not break request handling.
                }
            }

            private sealed class NullScope : IDisposable
            {
                internal static readonly NullScope Instance = new NullScope();
                public void Dispose() { }
            }
        }
    }
}
