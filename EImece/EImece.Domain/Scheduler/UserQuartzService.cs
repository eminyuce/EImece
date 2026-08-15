using EImece.Domain.Helpers;
using Quartz;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    /// <summary>
    /// Quartz service dedicated to customer and end-user scheduled background operations
    /// (e.g. abandoned cart reminders, price alerts, customer notifications).
    /// </summary>
    public class UserQuartzService : BaseQuartzService
    {
        public override async Task StartSchedulerServiceAsync()
        {
            Logger.Info("UserQuartzService has started");

            var quartzEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", true);
            if (!quartzEnabled)
            {
                Logger.Info("UserQuartzService skipped: Quartz_Scheduler_IsEnabled is false");
                return;
            }

            try
            {
                Logger.Info("UserQuartzService scheduling user jobs...");
                await ExecuteUserTasksAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "UserQuartzService failed to execute user tasks.");
            }
        }

        public async Task ExecuteUserTasksAsync()
        {
            IScheduler sched = Scheduler;
            if (sched == null)
            {
                Logger.Error("UserQuartzService.ExecuteUserTasksAsync: Scheduler is null");
                return;
            }

            await sched.Start().ConfigureAwait(false);

            // Scaffolded for future end-user scheduled jobs.
            // When end-user use cases are added, register them using ScheduleOrReschedule with Group = "User".
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
