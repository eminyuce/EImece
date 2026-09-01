using EImece.Domain.Configuration;
using EImece.Domain.Helpers;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Infrastructure
{
    [TestClass]
    public class MicrosoftExtensionsMigrationTests
    {
        static MicrosoftExtensionsMigrationTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve += TestAssemblyInitializer.OnAssemblyResolve;
        }

        [TestMethod]
        public async Task HealthChecks_RegisteredAndExecutable_ReturnsHealthReport()
        {
            var services = new ServiceCollection();
            services.AddEimeceOptions();
            services.AddEimeceHttpClients();
            services.AddEimeceHealthChecks();

            var provider = services.BuildServiceProvider();
            var healthCheckService = provider.GetRequiredService<HealthCheckService>();

            Assert.IsNotNull(healthCheckService);

            var report = await healthCheckService.CheckHealthAsync(CancellationToken.None);

            Assert.IsNotNull(report);
            Assert.IsTrue(report.Entries.ContainsKey(SqlServerHealthCheck.DefaultName));
            Assert.IsTrue(report.Entries.ContainsKey(FileStorageHealthCheck.DefaultName));
            Assert.IsTrue(report.Entries.ContainsKey(BackgroundServiceHealthCheck.DefaultName));
            Assert.IsTrue(report.Entries.ContainsKey(ExternalApiHealthCheck.DefaultName));
        }

        [TestMethod]
        public void ConfigurationJson_BuildsConfiguration_AndBindsOptions()
        {
            var services = new ServiceCollection();
            services.AddEimeceConfiguration();
            services.AddEimeceOptions();

            var provider = services.BuildServiceProvider();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var obsOptions = provider.GetRequiredService<ObservabilityOptions>();

            Assert.IsNotNull(configuration);
            Assert.IsNotNull(obsOptions);
        }

        [TestMethod]
        public void HttpPolly_RegistersNamedHttpClients_WithResilience()
        {
            var services = new ServiceCollection();
            services.AddEimeceOptions();
            services.AddEimeceHttpClients();

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpClientFactory>();

            Assert.IsNotNull(factory);

            var resilientClient = factory.CreateClient(HttpClientNames.Resilient);
            Assert.IsNotNull(resilientClient);

            var iyzicoClient = factory.CreateClient(HttpClientNames.Iyzico);
            Assert.IsNotNull(iyzicoClient);

            var recaptchaClient = factory.CreateClient(HttpClientNames.Recaptcha);
            Assert.IsNotNull(recaptchaClient);

            var externalClient = factory.CreateClient(HttpClientNames.ExternalApi);
            Assert.IsNotNull(externalClient);
        }

        [TestMethod]
        public void Localization_RegistersStringLocalizerFactory_InDI()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddLocalization(options => { options.ResourcesPath = "Resources"; });

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IStringLocalizerFactory>();

            Assert.IsNotNull(factory);
            var localizer = factory.Create("Common", "Resources");
            Assert.IsNotNull(localizer);
        }

        [TestMethod]
        public void ObjectPool_RentsAndReturnsStringBuilder_BuildsExpectedString()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<ObjectPool<StringBuilder>>(sp =>
                sp.GetRequiredService<ObjectPoolProvider>().Create(new StringBuilderPooledObjectPolicy()));

            var provider = services.BuildServiceProvider();
            var pool = provider.GetRequiredService<ObjectPool<StringBuilder>>();

            Assert.IsNotNull(pool);

            var sb = pool.Get();
            Assert.IsNotNull(sb);
            sb.Append("test-object-pooling");
            Assert.AreEqual("test-object-pooling", sb.ToString());
            pool.Return(sb);

            var result = ObjectPoolHelper.BuildString(builder =>
            {
                builder.Append("hello").Append(" ").Append("world");
            });

            Assert.AreEqual("hello world", result);
        }
    }
}
