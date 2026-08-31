using EImece.Domain.Helpers;
using EImece.Domain.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    /// <summary>
    /// Quartz service dedicated to system maintenance and administrative background jobs.
    /// </summary>
    public class AdminQuartzService : BaseQuartzService
    {
        public AdminQuartzService(IScheduler scheduler, ILogger<AdminQuartzService> logger)
            : base(scheduler, logger)
        {
        }

        public override async Task StartSchedulerServiceAsync()
        {
            Logger.LogInformation("AdminQuartzService has started");

            var quartzEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", true);
            if (!quartzEnabled)
            {
                Logger.LogInformation("AdminQuartzService skipped: Quartz_Scheduler_IsEnabled is false");
                return;
            }

            try
            {
                await ExecuteAdminTasksAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AdminQuartzService failed to execute admin tasks.");
            }
        }

        public async Task ExecuteAdminTasksAsync()
        {
            IScheduler sched = Scheduler;
            if (sched == null)
            {
                Logger.LogError("AdminQuartzService.ExecuteAdminTasksAsync: Scheduler is null");
                return;
            }

            await sched.Start().ConfigureAwait(false);

            var runningCronJobs = await sched.GetCurrentlyExecutingJobs().ConfigureAwait(false);
            foreach (var runningCronJob in runningCronJobs)
            {
                try
                {
                    var jobId = runningCronJob.JobDetail.Key.Name.Replace("Name-", "").ToInt();
                    Logger.LogInformation(
                        "Running cron job {JobName} {JobGroup} JobId={JobId}",
                        runningCronJob.JobDetail.Key.Name,
                        runningCronJob.JobDetail.Key.Group,
                        jobId);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error reading running cron job details");
                }
            }

            await ScheduleOrReschedule(sched, new ScheduleJob
            {
                JobId = 1,
                Group = "Admin",
                Name = "Testing",
                CronExp = "0 0/1 * * * ?",
                IsActive = true,
                TaskId = "Task-1"
            }, typeof(HelloJob)).ConfigureAwait(false);

            await ScheduleOrReschedule(sched, new ScheduleJob
            {
                JobId = 2,
                Group = "Admin",
                Name = "ClearExpiredShoppingCarts",
                CronExp = "0 30 2 */3 * ?",
                IsActive = true,
                TaskId = "Task-2"
            }, typeof(ClearExpiredShoppingCartsJob)).ConfigureAwait(false);

            await ScheduleOrReschedule(sched, new ScheduleJob
            {
                JobId = 3,
                Group = "Admin",
                Name = "ClearLogsFromDb",
                CronExp = "0 30 2 */7 * ?",
                IsActive = true,
                TaskId = "Task-3"
            }, typeof(ClearLogsFromDbJob)).ConfigureAwait(false);
        }
    }
}
