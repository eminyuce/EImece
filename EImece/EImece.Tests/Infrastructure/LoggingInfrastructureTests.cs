using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EImece.Tests.Infrastructure
{
    [TestClass]
    public class LoggingInfrastructureTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            LoggingOptions.ResetForTests();
            StructuredLoggingBootstrap.CloseAndFlush();
        }

        [TestMethod]
        public void LoggingOptions_DefaultFilePath_IsMediaLogs()
        {
            LoggingOptions.ResetForTests();
            var options = LoggingOptions.FromAppConfig();

            Assert.AreEqual("media/logs", options.FilePath);
            Assert.AreEqual("media/logs", LoggingOptions.DefaultFileRelativePath);
        }

        [TestMethod]
        public void LoggingOptions_ResolveAbsoluteLogDirectory_UsesAppBaseAndMediaLogs()
        {
            LoggingOptions.ResetForTests();
            var options = new LoggingOptions { FilePath = LoggingOptions.DefaultFileRelativePath };
            var expected = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "media", "logs");

            Assert.AreEqual(expected, options.ResolveAbsoluteLogDirectory());
        }

        [TestMethod]
        public void LoggingBootstrap_ConfiguresMelFactory_WithNLogBridge()
        {
            StructuredLoggingBootstrap.CloseAndFlush();
            var options = new LoggingOptions
            {
                MinimumLevel = LogLevel.Debug,
                FileEnabled = true,
                DatabaseEnabled = false,
                ConsoleEnabled = false,
                FilePath = LoggingOptions.DefaultFileRelativePath,
            };

            var factory = LoggingBootstrap.Configure(options);

            Assert.IsNotNull(factory);
            var logger = factory.CreateLogger("EImece.Tests.Logging");
            Assert.IsNotNull(logger);
            Assert.IsTrue(logger.IsEnabled(LogLevel.Information));
        }

        [TestMethod]
        public void StructuredLogging_BeginScope_IncludesCorrelationId()
        {
            StructuredLoggingBootstrap.CloseAndFlush();
            LoggingBootstrap.Configure(new LoggingOptions { DatabaseEnabled = false, FileEnabled = false });
            StructuredLoggingBootstrap.Configure(ObservabilityOptions.FromAppConfig());

            var correlationId = CorrelationIdContext.Ensure();
            using (StructuredLoggingBootstrap.BeginRequestScope())
            {
                Assert.AreEqual(correlationId, CorrelationIdContext.Current);
            }
        }

        [TestMethod]
        public void FailSafeLoggerProvider_IsolatesSinkFailures()
        {
            var inner = new ThrowingLoggerProvider();
            var provider = new FailSafeLoggerProvider(inner);
            var logger = provider.CreateLogger("fail-safe-test");

            logger.LogInformation("should not throw");
            Assert.IsTrue(inner.CreateAttempts >= 1);
        }

        [TestMethod]
        public void WebConfig_LoggingFilePath_DefaultsToMediaLogs()
        {
            var webConfigPath = LocateWebConfig();
            if (webConfigPath == null)
            {
                Assert.Inconclusive("Web.config not found in test environment.");
                return;
            }

            var doc = XDocument.Load(webConfigPath);
            var filePath = doc.Descendants("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == "Logging:File:Path")
                ?.Attribute("value")?.Value;

            Assert.AreEqual("media/logs", filePath);
        }

        private static string LocateWebConfig()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                var candidate = Path.Combine(dir, "EImece", "Web.config");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                candidate = Path.Combine(dir, "Web.config");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            return null;
        }

        private sealed class ThrowingLoggerProvider : ILoggerProvider
        {
            public int CreateAttempts { get; private set; }

            public ILogger CreateLogger(string categoryName)
            {
                CreateAttempts++;
                return new ThrowingLogger();
            }

            public void Dispose()
            {
            }
        }

        private sealed class ThrowingLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                throw new InvalidOperationException("Simulated sink failure.");
            }

            private sealed class NullScope : IDisposable
            {
                internal static readonly NullScope Instance = new NullScope();
                public void Dispose() { }
            }
        }
    }
}
