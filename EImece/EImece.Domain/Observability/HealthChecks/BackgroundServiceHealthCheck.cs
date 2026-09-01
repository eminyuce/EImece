using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class BackgroundServiceHealthCheck : IHealthCheck
    {
        public const string DefaultName = "backgroundServices";

        private readonly IScheduler _scheduler;

        public BackgroundServiceHealthCheck(IScheduler scheduler = null)
        {
            _scheduler = scheduler;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var isEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", false);

            // Config says scheduler should be off → report as Healthy (by design)
            if (!isEnabled)
            {
                return HealthCheckResult.Healthy("Quartz scheduler disabled by config");
            }

            try
            {
                if (_scheduler == null)
                {
                    return HealthCheckResult.Unhealthy("IScheduler is not registered / null");
                }

                // Real runtime state
                if (_scheduler.IsShutdown)
                {
                    return HealthCheckResult.Unhealthy("Quartz scheduler is shut down");
                }

                if (!_scheduler.IsStarted)
                {
                    return HealthCheckResult.Unhealthy("Quartz scheduler is not started");
                }

                if (_scheduler.InStandbyMode)
                {
                    return HealthCheckResult.Unhealthy("Quartz scheduler is in standby mode");
                }

                var jobKeys = await _scheduler.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup(), cancellationToken).ConfigureAwait(false);
                var executing = await _scheduler.GetCurrentlyExecutingJobs(cancellationToken).ConfigureAwait(false);

                var registeredCount = jobKeys != null ? jobKeys.Count : 0;
                var executingCount = executing != null ? executing.Count : 0;
                var detail = string.Format("Quartz scheduler running ({0} jobs registered, {1} currently executing)", registeredCount, executingCount);

                var data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "SchedulerName", _scheduler.SchedulerName },
                    { "IsStarted", _scheduler.IsStarted },
                    { "InStandbyMode", _scheduler.InStandbyMode },
                    { "IsShutdown", _scheduler.IsShutdown },
                    { "RegisteredJobsCount", registeredCount },
                    { "CurrentlyExecutingCount", executingCount }
                };

                return HealthCheckResult.Healthy(detail, data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Quartz health check failed: " + ex.Message, ex);
            }
        }
    }
}
