using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Configuration;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Http;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Metrics;
using EImece.Tests.Infrastructure;
using EImece.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Net.Http;

namespace EImece.Tests.Infrastructure
{
    [TestClass]
    public class MelInfrastructureTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            AppConfig.ResetCacheForTests();
            CacheOptions.ResetForTests();
            IyzicoOptions.ResetForTests();
            OutboundHttpOptions.ResetForTests();
            LoggingOptions.ResetForTests();
            ObservabilityOptions.ResetForTests();
        }

        [TestMethod]
        public void ServiceCollection_ResolvesInfrastructureOptions_FromAppConfig()
        {
            ConfigurationManager.AppSettings["IyzicoBaseUrl"] = "https://test-iyzico.example/";
            ConfigurationManager.AppSettings["IsCacheActive"] = "true";
            IyzicoOptions.ResetForTests();
            CacheOptions.ResetForTests();

            var services = new ServiceCollection();
            services.AddEimeceOptions();

            using (var provider = services.BuildServiceProvider(validateScopes: true))
            {
                var iyzico = provider.GetRequiredService<IOptions<IyzicoOptions>>().Value;
                Assert.AreEqual("https://test-iyzico.example/", iyzico.BaseUrl);

                var cache = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
                Assert.IsTrue(cache.IsActive);
                Assert.AreEqual(900, cache.LongSeconds);
            }
        }

        [TestMethod]
        public void ServiceCollection_ResolvesConcreteAndIOptions_WithValidateScopes()
        {
            var services = new ServiceCollection();
            services.AddEimeceOptions();

            using (var provider = services.BuildServiceProvider(validateScopes: true))
            {
                var outboundConcrete = provider.GetRequiredService<OutboundHttpOptions>();
                var outboundWrapped = provider.GetRequiredService<IOptions<OutboundHttpOptions>>().Value;
                Assert.IsNotNull(outboundConcrete);
                Assert.AreSame(outboundConcrete, outboundWrapped);

                var iyzicoConcrete = provider.GetRequiredService<IyzicoOptions>();
                var iyzicoWrapped = provider.GetRequiredService<IOptions<IyzicoOptions>>().Value;
                Assert.IsNotNull(iyzicoConcrete);
                Assert.AreSame(iyzicoConcrete, iyzicoWrapped);

                var cacheConcrete = provider.GetRequiredService<CacheOptions>();
                var cacheWrapped = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
                Assert.IsNotNull(cacheConcrete);
                Assert.AreSame(cacheConcrete, cacheWrapped);
            }
        }

        [TestMethod]
        public void RecaptchaService_Configure_SucceedsWithCompositionRootRegistrations()
        {
            var services = new ServiceCollection();
            services.AddEimeceOptions();
            services.AddEimeceHttpClients();

            using (var provider = services.BuildServiceProvider(validateScopes: true))
            {
                RecaptchaService.Configure(provider);
            }
        }

        [TestMethod]
        public void ServiceCollection_RegistersNamedHttpClients()
        {
            var services = new ServiceCollection();
            services.AddEimeceOptions();
            services.AddEimeceHttpClients();

            using (var provider = services.BuildServiceProvider())
            {
                var factory = provider.GetRequiredService<IHttpClientFactory>();

                var resilient = factory.CreateClient(HttpClientNames.Resilient);
                var recaptcha = factory.CreateClient(HttpClientNames.Recaptcha);
                var external = factory.CreateClient(HttpClientNames.ExternalApi);

                Assert.IsNotNull(resilient);
                Assert.IsNotNull(recaptcha);
                Assert.IsNotNull(external);
                Assert.AreNotSame(resilient, recaptcha);
            }
        }

        [TestMethod]
        public void LazyCacheProvider_CacheHit_SkipsLoader_AndInvalidatesAfterClear()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMemoryCache();
            services.AddSingleton<IEimeceCacheProvider, LazyCacheProvider>();

            using (var provider = services.BuildServiceProvider())
            {
                var cache = provider.GetRequiredService<IEimeceCacheProvider>();
                var key = "MelInfrastructureTests:settings";
                var loadCount = 0;

                var first = cache.GetOrAdd(key, () =>
                {
                    loadCount++;
                    return "value-v1";
                }, 300);

                var second = cache.GetOrAdd(key, () =>
                {
                    loadCount++;
                    return "value-v2";
                }, 300);

                Assert.AreEqual("value-v1", first);
                Assert.AreEqual("value-v1", second);
                Assert.AreEqual(1, loadCount, "Cache hit should not invoke the loader again.");

                cache.Clear(key);

                var third = cache.GetOrAdd(key, () =>
                {
                    loadCount++;
                    return "value-v3";
                }, 300);

                Assert.AreEqual("value-v3", third);
                Assert.AreEqual(2, loadCount, "Invalidation should force a reload on the next miss.");
            }
        }

        [TestMethod]
        public void ResilientHttpClient_UsesFactoryManagedClient_AndRemainsSingleton()
        {
            var services = new ServiceCollection();
            services.AddEimeceOptions();
            services.AddEimeceHttpClients();
            services.AddLogging(builder => builder.AddProvider(new NullLoggerProvider()));
            services.AddSingleton<IApplicationMetrics, ApplicationMetrics>();
            services.AddSingleton<ResilientHttpClient>();
            services.AddSingleton<IResilientHttpClient>(sp => sp.GetRequiredService<ResilientHttpClient>());

            using (var provider = services.BuildServiceProvider())
            {
                var client1 = provider.GetRequiredService<IResilientHttpClient>();
                var client2 = provider.GetRequiredService<IResilientHttpClient>();
                Assert.AreSame(client1, client2);
            }
        }

        [TestMethod]
        public void ILogger_StillResolvesThroughMelFactory()
        {
            StructuredLoggingBootstrap.CloseAndFlush();
            var services = new ServiceCollection();
            services.AddEimeceOptions();
            services.AddSingleton<ILoggerFactory>(sp =>
            {
                var options = sp.GetRequiredService<LoggingOptions>();
                return LoggingBootstrap.Configure(new LoggingOptions
                {
                    MinimumLevel = LogLevel.Information,
                    FileEnabled = false,
                    DatabaseEnabled = false,
                    ConsoleEnabled = false,
                    FilePath = LoggingOptions.DefaultFileRelativePath,
                });
            });
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            using (var provider = services.BuildServiceProvider())
            {
                var logger = provider.GetRequiredService<ILogger<MelInfrastructureTests>>();
                Assert.IsNotNull(logger);
                Assert.IsTrue(logger.IsEnabled(LogLevel.Information));
            }
        }

        private sealed class NullLoggerProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => TestNullLoggers.Create();

            public void Dispose()
            {
            }
        }
    }
}
