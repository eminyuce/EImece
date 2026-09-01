using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Configuration
{
    /// <summary>
    /// Registers high-value infrastructure options from IConfiguration (appsettings.json / environment)
    /// with seamless fallback to legacy Web.config / AppConfig.
    /// </summary>
    public static class EimeceOptionsRegistration
    {
        public static IServiceCollection AddEimeceOptions(this IServiceCollection services)
        {
            RegisterOptions(services, "Logging", LoggingOptions.FromAppConfig);
            RegisterOptions(services, "Observability", ObservabilityOptions.FromAppConfig);
            RegisterOptions(services, "Iyzico", IyzicoOptions.FromAppConfig);
            RegisterOptions(services, "Cache", CacheOptions.FromAppConfig);
            RegisterOptions(services, "OutboundHttp", OutboundHttpOptions.FromAppConfig);
            return services;
        }

        private static void RegisterOptions<TOptions>(IServiceCollection services, string sectionName, System.Func<TOptions> fallbackFactory)
            where TOptions : class, new()
        {
            services.AddSingleton<TOptions>(sp =>
            {
                var config = sp.GetService<IConfiguration>();
                if (config != null && !string.IsNullOrWhiteSpace(sectionName))
                {
                    var section = config.GetSection(sectionName);
                    if (section.Exists())
                    {
                        var options = new TOptions();
                        section.Bind(options);
                        return options;
                    }
                }
                return fallbackFactory();
            });
            services.AddSingleton<IOptions<TOptions>>(sp => Options.Create(sp.GetRequiredService<TOptions>()));
            services.AddSingleton<IOptionsMonitor<TOptions>>(sp => new OptionsMonitorWrapper<TOptions>(sp.GetRequiredService<TOptions>()));
        }

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
