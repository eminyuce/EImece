using Castle.DynamicProxy;
using System;

namespace EImece.Domain.Observability.Telemetry
{
    /// <summary>
    /// Simple static helper to create Castle DynamicProxy proxies with <see cref="TimedInterceptor"/>.
    /// All created proxies measure methods marked with <see cref="TimedAttribute"/> (service/repo).
    /// Methods MUST be virtual for class proxies, or use interface proxies.
    /// </summary>
    /// <example>
    /// Manual:
    /// <code>
    /// var repo = new ConversationRepository();
    /// var timedRepo = ProxyFactory.Create(repo); // repo.GetByUser now timed if decorated
    /// var svc = new ConversationService(timedRepo);
    /// var timedSvc = ProxyFactory.Create(svc);
    /// var conversations = timedSvc.GetConversations(42);
    /// </code>
    /// Autofac:
    /// <code>
    /// builder.RegisterType&lt;TimedInterceptor&gt;().SingleInstance();
    /// builder.RegisterType&lt;ConversationService&gt;().AsSelf().EnableClassInterceptors().InterceptedBy&lt;TimedInterceptor&gt;();
    /// builder.RegisterType&lt;ConversationRepository&gt;().AsSelf().EnableClassInterceptors().InterceptedBy&lt;TimedInterceptor&gt;();
    /// </code>
    /// MS DependencyInjection (pure):
    /// <code>
    /// services.AddSingleton&lt;TimedInterceptor&gt;();
    /// // decorate after build:
    /// var repo = ProxyFactory.Create(new ConversationRepository());
    /// services.AddSingleton&lt;ConversationRepository&gt;(repo);
    /// </code>
    /// </example>
    public static class ProxyFactory
    {
        // ProxyGenerator is thread-safe after construction; reuse singleton for performance.
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        /// <summary>
        /// Creates a class proxy with target (requires virtual methods).
        /// </summary>
        public static T Create<T>(T target) where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return Generator.CreateClassProxyWithTarget(target, new TimedInterceptor());
        }

        /// <summary>
        /// Creates a class proxy with target using a shared interceptor instance.
        /// </summary>
        public static T Create<T>(T target, TimedInterceptor interceptor) where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (interceptor == null) throw new ArgumentNullException(nameof(interceptor));
            return Generator.CreateClassProxyWithTarget(target, interceptor);
        }

        /// <summary>
        /// Creates an interface proxy with target (methods need not be virtual, proxy is via interface).
        /// </summary>
        public static T CreateInterface<T>(T target) where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            // T is an interface; need to ensure it is actually an interface.
            return Generator.CreateInterfaceProxyWithTarget(target, new TimedInterceptor());
        }

        /// <summary>
        /// Creates an interface proxy with explicit interceptor instance.
        /// </summary>
        public static T CreateInterface<T>(T target, TimedInterceptor interceptor) where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (interceptor == null) throw new ArgumentNullException(nameof(interceptor));
            return Generator.CreateInterfaceProxyWithTarget(target, interceptor);
        }
    }
}
