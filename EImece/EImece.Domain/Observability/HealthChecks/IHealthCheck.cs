using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public interface IHealthCheck
    {
        string Name { get; }

        Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken);
    }
}
