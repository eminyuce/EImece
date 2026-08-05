using EImece.Domain.Observability.Metrics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public interface IHealthCheckService
    {
        Task<HealthCheckResponse> GetHealthAsync(CancellationToken cancellationToken);
    }

    public sealed class HealthCheckService : IHealthCheckService
    {
        private readonly IReadOnlyList<IHealthCheck> _healthChecks;

        public HealthCheckService(IEnumerable<IHealthCheck> healthChecks)
        {
            _healthChecks = healthChecks.ToList();
        }

        public async Task<HealthCheckResponse> GetHealthAsync(CancellationToken cancellationToken)
        {
            var results = new List<HealthCheckResult>(_healthChecks.Count);

            foreach (var healthCheck in _healthChecks)
            {
                var result = await healthCheck.CheckAsync(cancellationToken).ConfigureAwait(false);

                // Do not include checks that are explicitly "not configured" to keep the response clean
                if (result == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(result.Message) &&
                    result.Message.Equals("not configured", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                results.Add(result);
            }

            var response = HealthCheckResponse.Create(results);
            OpenTelemetryMetrics.SetHealthStatus(
                string.Equals(response.Status, "UP", System.StringComparison.OrdinalIgnoreCase));
            return response;
        }
    }
}
