using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class BackgroundServiceHealthCheck : IHealthCheck
    {
        public string Name
        {
            get { return "backgroundServices"; }
        }

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var isEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", false);
            if (!isEnabled)
            {
                return Task.FromResult(HealthCheckResult.Up(Name, "scheduler disabled"));
            }

            return Task.FromResult(HealthCheckResult.Up(Name, "scheduler enabled"));
        }
    }
}
