using EImece.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    /// <summary>
    /// Quartz service dedicated to customer and end-user scheduled background operations.
    /// </summary>
    public class UserQuartzService : BaseQuartzService
    {
        public UserQuartzService(IScheduler scheduler, ILogger<UserQuartzService> logger)
            : base(scheduler, logger)
        {
        }

        public override async Task StartSchedulerServiceAsync()
        {
            Logger.LogInformation("UserQuartzService has started");

            var quartzEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", true);
            if (!quartzEnabled)
            {
                Logger.LogInformation("UserQuartzService skipped: Quartz_Scheduler_IsEnabled is false");
                return;
            }

            try
            {
                await ExecuteUserTasksAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UserQuartzService failed to execute user tasks.");
            }
        }

        public Task ExecuteUserTasksAsync()
        {
            IScheduler sched = Scheduler;
            if (sched == null)
            {
                Logger.LogError("UserQuartzService.ExecuteUserTasksAsync: Scheduler is null");
                return Task.CompletedTask;
            }

            return sched.Start();
        }
    }
}
