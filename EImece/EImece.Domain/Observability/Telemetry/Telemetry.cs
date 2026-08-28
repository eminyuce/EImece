using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace EImece.Domain.Observability.Telemetry
{
    /// <summary>
    /// Lightweight business-metric helper for ASP.NET MVC 5 / .NET Framework 4.8.
    /// Relies on the <see cref="OpenTelemetryBootstrap"/> MeterProvider for export
    /// (OTLP / Azure Monitor / Console) — no extra wiring needed.
    /// Keeps a concurrent cache so Meter.CreateHistogram is called once per metric name.
    /// Overall HTTP request duration is already covered by OpenTelemetry.Instrumentation.AspNet;
    /// use this helper only for opt-in business-oriented histograms via [Timed].
    /// </summary>
    public static class Telemetry
    {
        // Fallback meter for tests / very early startup before OpenTelemetryBootstrap.Initialize().
        // Uses the same instrument name ("EImece") as OpenTelemetryBootstrap.MeterName so
        // MeterProvider.AddMeter("EImece") already captures it. Replace "EImece"/version with
        // your company/app identifier if you fork this helper (e.g. "MyCompany.MyApp", "1.0.0").
        private static readonly Meter FallbackMeter = new Meter("EImece", "1.0.0");

        /// <summary>
        /// Application Meter. Prefers the bootstrap Meter when initialized; falls back to a
        /// standalone Meter with the same name so unit tests can still record without bootstrapping.
        /// </summary>
        public static Meter Meter
        {
            get
            {
                try
                {
                    var bootstrapMeter = OpenTelemetryBootstrap.Meter;
                    return bootstrapMeter ?? FallbackMeter;
                }
                catch
                {
                    return FallbackMeter;
                }
            }
        }

        // Histogram cache — ConcurrentDictionary makes GetOrAdd safe under concurrent requests.
        private static readonly ConcurrentDictionary<string, Histogram<double>> Histograms =
            new ConcurrentDictionary<string, Histogram<double>>(StringComparer.Ordinal);

        /// <summary>
        /// Gets an existing Histogram or creates a new one. Thread-safe.
        /// </summary>
        /// <param name="name">Metric name, e.g. "service.conversations.getConversations". Must be non-empty.</param>
        /// <param name="description">Optional description shown in metric exporters.</param>
        /// <returns>Cached Histogram instance.</returns>
        public static Histogram<double> GetOrCreateHistogram(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Histogram name must not be empty.", nameof(name));
            }

            // Meter.CreateHistogram is cheap but we cache to avoid repeated lookups.
            return Histograms.GetOrAdd(name, n =>
                Meter.CreateHistogram<double>(n, unit: "ms", description: description));
        }
    }
}
