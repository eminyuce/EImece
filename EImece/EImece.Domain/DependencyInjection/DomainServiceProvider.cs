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

        public static T GetService<T>() where T : class
        {
            return Instance?.GetService(typeof(T)) as T;
        }
    }
}
