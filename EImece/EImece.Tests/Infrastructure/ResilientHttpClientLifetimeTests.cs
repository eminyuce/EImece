using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Http;
using EImece.Domain.Observability.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EImece.Tests.Infrastructure
{
    [TestClass]
    public class ResilientHttpClientLifetimeTests
    {
        private IServiceProvider CreateTestServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton(_ => ObservabilityOptions.FromAppConfig());
            services.AddSingleton<IApplicationMetrics, ApplicationMetrics>();
            services.AddSingleton<ILoggerFactory>(_ => new LoggerFactory());
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Register ResilientHttpClient as Singleton
            services.AddSingleton<ResilientHttpClient>();
            services.AddSingleton<IResilientHttpClient>(sp => sp.GetRequiredService<ResilientHttpClient>());

            return services.BuildServiceProvider(validateScopes: true);
        }

        [TestMethod]
        public void ResilientHttpClient_ResolvedFromRoot_IsSingleton()
        {
            // Arrange
            var provider = CreateTestServiceProvider();

            // Act
            var client1 = provider.GetRequiredService<IResilientHttpClient>();
            var client2 = provider.GetRequiredService<IResilientHttpClient>();
            var concreteClient = provider.GetRequiredService<ResilientHttpClient>();

            // Assert
            Assert.IsNotNull(client1);
            Assert.IsNotNull(client2);
            Assert.AreSame(client1, client2, "IResilientHttpClient must resolve to the same singleton instance.");
            Assert.AreSame(client1, concreteClient, "Interface and concrete ResilientHttpClient must resolve to the same singleton instance.");
        }

        [TestMethod]
        public void ResilientHttpClient_ResolvedAcrossDifferentScopes_IsSingleton()
        {
            // Arrange
            var rootProvider = CreateTestServiceProvider();

            IResilientHttpClient clientFromScope1;
            IResilientHttpClient clientFromScope2;

            // Act: Resolve in scope 1
            using (var scope1 = rootProvider.CreateScope())
            {
                clientFromScope1 = scope1.ServiceProvider.GetRequiredService<IResilientHttpClient>();
            }

            // Act: Resolve in scope 2
            using (var scope2 = rootProvider.CreateScope())
            {
                clientFromScope2 = scope2.ServiceProvider.GetRequiredService<IResilientHttpClient>();
            }

            // Assert
            Assert.IsNotNull(clientFromScope1);
            Assert.IsNotNull(clientFromScope2);
            Assert.AreSame(clientFromScope1, clientFromScope2, "IResilientHttpClient must remain the same singleton across different request/service scopes.");
        }
    }
}
