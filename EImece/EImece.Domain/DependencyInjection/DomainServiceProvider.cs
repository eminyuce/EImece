using System;

namespace EImece.Domain.DependencyInjection
{
    /// <summary>
    /// Provides access to the root <see cref="IServiceProvider"/> for background services and Quartz jobs
    /// that execute outside of an HTTP request lifecycle.
    /// </summary>
    public static class DomainServiceProvider
    {
        public static IServiceProvider Instance { get; set; }

        /// <summary>
        /// Optional current-request (or ambient) provider. When set, scoped services resolve from
        /// the request scope instead of the root provider.
        /// </summary>
        public static Func<IServiceProvider> RequestProvider { get; set; }

        public static T GetService<T>() where T : class
        {
            var sp = RequestProvider?.Invoke() ?? Instance;
            return sp?.GetService(typeof(T)) as T;
        }
    }
}