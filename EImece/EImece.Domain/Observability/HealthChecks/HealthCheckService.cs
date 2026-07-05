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
                results.Add(await healthCheck.CheckAsync(cancellationToken).ConfigureAwait(false));
            }

            return HealthCheckResponse.Create(results);
        }
    }
}
