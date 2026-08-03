using EImece.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    /// <summary>
    /// Reproduces the MS.DI CallSiteRuntimeResolver failure
    /// ("An item with the same key has already been added") that occurs when
    /// scoped services with circular [Inject] properties are resolved without
    /// short-circuiting in-flight instances (e.g. ProductService ↔ ProductCategoryService).
    /// </summary>
    [TestClass]
    public class PropertyInjectorCircularDependencyTests
    {
        private interface IServiceA
        {
            IServiceB B { get; }
        }

        private interface IServiceB
        {
            IServiceA A { get; }
        }

        private sealed class ServiceA : IServiceA
        {
            [Inject]
            public IServiceB B { get; set; }
        }

        private sealed class ServiceB : IServiceB
        {
            [Inject]
            public IServiceA A { get; set; }
        }

        [TestMethod]
        public void Create_CircularInjectProperties_SharesScopedInstances()
        {
            var services = new ServiceCollection();
            services.AddScoped<ServiceA>(sp => PropertyInjector.Create<ServiceA>(sp));
            services.AddScoped<IServiceA>(sp =>
            {
                var underConstruction = PropertyInjector.TryGetUnderConstruction(typeof(ServiceA));
                if (underConstruction is IServiceA typed)
                {
                    return typed;
                }

                return sp.GetRequiredService<ServiceA>();
            });
            services.AddScoped<ServiceB>(sp => PropertyInjector.Create<ServiceB>(sp));
            services.AddScoped<IServiceB>(sp =>
            {
                var underConstruction = PropertyInjector.TryGetUnderConstruction(typeof(ServiceB));
                if (underConstruction is IServiceB typed)
                {
                    return typed;
                }

                return sp.GetRequiredService<ServiceB>();
            });

            var provider = services.BuildServiceProvider(validateScopes: true);
            using (var scope = provider.CreateScope())
            {
                var a = scope.ServiceProvider.GetRequiredService<IServiceA>();
                var b = scope.ServiceProvider.GetRequiredService<IServiceB>();

                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                Assert.IsNotNull(a.B);
                Assert.IsNotNull(b.A);
                Assert.AreSame(a, b.A);
                Assert.AreSame(b, a.B);
                Assert.AreSame(a, scope.ServiceProvider.GetRequiredService<IServiceA>());
                Assert.AreSame(b, scope.ServiceProvider.GetRequiredService<IServiceB>());
            }
        }
    }
}
