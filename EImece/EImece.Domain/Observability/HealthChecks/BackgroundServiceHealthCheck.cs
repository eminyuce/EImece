using EImece.Domain.DependencyInjection;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class BackgroundServiceHealthCheck : IHealthCheck
    {
        private readonly IScheduler Scheduler;

        public BackgroundServiceHealthCheck(IScheduler scheduler = null)
        {
            Scheduler = scheduler;
        }

        public string Name
        {
            get { return "backgroundServices"; }
        }

        public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var isEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", false);

            // Config says scheduler should be off → report as Up (by design)
            if (!isEnabled)
            {
                return HealthCheckResult.Up(Name, "Quartz scheduler disabled by config");
            }

            try
            {
                if (Scheduler == null)
                {
                    return HealthCheckResult.Down(Name, "IScheduler is not registered / null");
                }

                // Real runtime state
                if (Scheduler.IsShutdown)
                {
                    return HealthCheckResult.Down(Name, "Quartz scheduler is shut down");
                }

                if (!Scheduler.IsStarted)
                {
                    return HealthCheckResult.Down(Name, "Quartz scheduler is not started");
                }

                if (Scheduler.InStandbyMode)
                {
                    return HealthCheckResult.Down(Name, "Quartz scheduler is in standby mode");
                }

                var jobKeys = await Scheduler.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup(), cancellationToken).ConfigureAwait(false);
                var executing = await Scheduler.GetCurrentlyExecutingJobs(cancellationToken).ConfigureAwait(false);

                var detail = string.Format(
                    "Quartz running. Jobs registered: {0}, currently executing: {1}",
                    jobKeys != null ? jobKeys.Count : 0,
                    executing != null ? executing.Count : 0);

                return HealthCheckResult.Up(Name, detail);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Down(Name, "Quartz health check failed: " + ex.Message);
            }
        }
    }
}
