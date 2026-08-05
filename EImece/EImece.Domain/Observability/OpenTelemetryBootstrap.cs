using Azure.Monitor.OpenTelemetry.Exporter;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace EImece.Domain.Observability
{
    /// <summary>
    /// Vendor-neutral OpenTelemetry bootstrap for .NET Framework 4.8.1 / ASP.NET MVC 5.
    /// Safe no-op when tracing and metrics are disabled or no exporters are configured.
    /// </summary>
    public sealed class OpenTelemetryBootstrap : IDisposable
    {
        public const string ActivitySourceName = "EImece";
        public const string MeterName = "EImece";

        private static readonly object Sync = new object();
        private static OpenTelemetryBootstrap _instance;

        private readonly TracerProvider _tracerProvider;
        private readonly MeterProvider _meterProvider;
        private bool _disposed;

        public static ActivitySource ActivitySource { get; private set; } =
            new ActivitySource(ActivitySourceName, GetFallbackVersion());

        public static Meter Meter { get; private set; } =
            new Meter(MeterName, GetFallbackVersion());

        public static bool IsInitialized
        {
            get { return _instance != null; }
        }

        private OpenTelemetryBootstrap(TracerProvider tracerProvider, MeterProvider meterProvider)
        {
            _tracerProvider = tracerProvider;
            _meterProvider = meterProvider;
        }

        /// <summary>
        /// Initializes ActivitySource, Meter, TracerProvider, and MeterProvider once per AppDomain.
        /// </summary>
        public static OpenTelemetryBootstrap Initialize(ObservabilityOptions options = null)
        {
            lock (Sync)
            {
                if (_instance != null)
                {
                    return _instance;
                }

                options = options ?? ObservabilityOptions.FromAppConfig();
                var version = string.IsNullOrWhiteSpace(options.ServiceVersion)
                    ? GetFallbackVersion()
                    : options.ServiceVersion.Trim();

                ActivitySource?.Dispose();
                Meter?.Dispose();
                ActivitySource = new ActivitySource(ActivitySourceName, version);
                Meter = new Meter(MeterName, version);
                OpenTelemetryMetrics.Initialize(Meter);

                TracerProvider tracerProvider = null;
                MeterProvider meterProvider = null;

                if (options.EnableTracing || options.EnableMetrics)
                {
                    var resource = BuildResource(options, version);

                    if (options.EnableTracing)
                    {
                        tracerProvider = BuildTracerProvider(options, resource);
                    }

                    if (options.EnableMetrics)
                    {
                        meterProvider = BuildMeterProvider(options, resource);
                    }
                }

                _instance = new OpenTelemetryBootstrap(tracerProvider, meterProvider);
                return _instance;
            }
        }

        public static void Shutdown()
        {
            lock (Sync)
            {
                if (_instance == null)
                {
                    return;
                }

                _instance.Dispose();
                _instance = null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _tracerProvider?.Shutdown();
            }
            catch
            {
                // Best-effort during AppDomain recycle.
            }

            try
            {
                _meterProvider?.Shutdown();
            }
            catch
            {
            }

            _tracerProvider?.Dispose();
            _meterProvider?.Dispose();
        }

        private static ResourceBuilder BuildResource(ObservabilityOptions options, string version)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("deployment.environment", options.DeploymentEnvironment ?? "dev")
            };

            return ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: string.IsNullOrWhiteSpace(options.ServiceName) ? "EImece" : options.ServiceName,
                    serviceVersion: version)
                .AddAttributes(attributes);
        }

        private static TracerProvider BuildTracerProvider(ObservabilityOptions options, ResourceBuilder resource)
        {
            var sampler = new ParentBasedSampler(new TraceIdRatioBasedSampler(options.SamplingRatio));

            var builder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resource)
                .SetSampler(sampler)
                .AddSource(ActivitySourceName)
                .AddHttpClientInstrumentation(http =>
                {
                    http.FilterHttpRequestMessage = _ => true;
                    http.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        if (activity == null || request?.RequestUri == null)
                        {
                            return;
                        }

                        // Low-cardinality host only — never full URL with query/secrets.
                        activity.SetTag("server.address", request.RequestUri.Host);
                        activity.SetTag("http.request.method", request.Method?.Method);
                    };
                    http.EnrichWithHttpResponseMessage = (activity, response) =>
                    {
                        if (activity == null || response == null)
                        {
                            return;
                        }

                        activity.SetTag("http.response.status_code", (int)response.StatusCode);
                    };
                });

            if (options.HasOtlpExporter)
            {
                builder.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri(options.OtlpEndpoint.Trim());
                });
            }

            if (options.HasAzureMonitorExporter)
            {
                builder.AddAzureMonitorTraceExporter(azure =>
                {
                    azure.ConnectionString = options.AzureMonitorConnectionString;
                });
            }

            // When no exporter is configured, still build a provider so Activities are sampled
            // locally for log enrichment without outbound I/O.
            return builder.Build();
        }

        private static MeterProvider BuildMeterProvider(ObservabilityOptions options, ResourceBuilder resource)
        {
            var builder = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resource)
                .AddMeter(MeterName)
                .AddHttpClientInstrumentation();

            if (options.HasOtlpExporter)
            {
                builder.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri(options.OtlpEndpoint.Trim());
                });
            }

            if (options.HasAzureMonitorExporter)
            {
                builder.AddAzureMonitorMetricExporter(azure =>
                {
                    azure.ConnectionString = options.AzureMonitorConnectionString;
                });
            }

            return builder.Build();
        }

        private static string GetFallbackVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrWhiteSpace(informational))
            {
                var plus = informational.IndexOf('+');
                return plus > 0 ? informational.Substring(0, plus) : informational;
            }

            return assembly.GetName().Version?.ToString() ?? "1.0.0";
        }
    }
}
