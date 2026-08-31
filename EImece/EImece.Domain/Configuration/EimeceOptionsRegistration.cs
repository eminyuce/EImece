using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Configuration
{
    /// <summary>
    /// Registers high-value infrastructure options from Web.config / environment (not appsettings.json).
    /// </summary>
    public static class EimeceOptionsRegistration
    {
        public static IServiceCollection AddEimeceOptions(this IServiceCollection services)
        {
            RegisterOptions(services, LoggingOptions.FromAppConfig);
            RegisterOptions(services, ObservabilityOptions.FromAppConfig);
            RegisterOptions(services, IyzicoOptions.FromAppConfig);
            RegisterOptions(services, CacheOptions.FromAppConfig);
            RegisterOptions(services, OutboundHttpOptions.FromAppConfig);
            return services;
        }

        private static void RegisterOptions<TOptions>(IServiceCollection services, System.Func<TOptions> factory)
            where TOptions : class
        {
            services.AddSingleton(factory);
            services.AddSingleton<IOptions<TOptions>>(sp => Options.Create(sp.GetRequiredService<TOptions>()));
            services.AddSingleton<IOptionsMonitor<TOptions>>(sp => new OptionsMonitorWrapper<TOptions>(sp.GetRequiredService<TOptions>()));
        }

        /// <summary>
        /// Web.config values are fixed for the AppDomain lifetime; monitor is a thin IOptions adapter.
        /// </summary>
        private sealed class OptionsMonitorWrapper<TOptions> : IOptionsMonitor<TOptions>
            where TOptions : class
        {
            private readonly TOptions _value;

            public OptionsMonitorWrapper(TOptions value)
            {
                _value = value;
            }

            public TOptions CurrentValue => _value;

            public TOptions Get(string name) => _value;

            public System.IDisposable OnChange(System.Action<TOptions, string> listener) => NullDisposable.Instance;

            private sealed class NullDisposable : System.IDisposable
            {
                internal static readonly NullDisposable Instance = new NullDisposable();

                public void Dispose()
                {
                }
            }
        }
    }
}
