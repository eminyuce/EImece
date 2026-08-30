using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Metrics;

namespace EImece.Web.Filters
{
    /// <summary>
    /// Backward-compatible alias for <see cref="TelemetryActionFilter"/>.
    /// Prefer registering <see cref="TelemetryActionFilter"/> directly.
    /// </summary>
    public sealed class MetricsActionFilter : TelemetryActionFilter
    {
        public MetricsActionFilter(IApplicationMetrics metrics)
            : base(metrics, ObservabilityOptions.FromAppConfig())
        {
        }
    }
}
