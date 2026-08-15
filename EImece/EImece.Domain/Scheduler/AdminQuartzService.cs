using EImece.Domain.Helpers;
using EImece.Domain.Scheduler.Jobs;
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
        public override async Task StartSchedulerServiceAsync()
        {
            Logger.Info("AdminQuartzService has started");

            var quartzEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", true);
            if (!quartzEnabled)
            {
                Logger.Info("AdminQuartzService skipped: Quartz_Scheduler_IsEnabled is false");
                return;
            }

            try
            {
                Logger.Info("AdminQuartzService scheduling admin jobs...");
                await ExecuteAdminTasksAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AdminQuartzService failed to execute admin tasks.");
            }
        }

        public async Task ExecuteAdminTasksAsync()
        {
            IScheduler sched = Scheduler;
            if (sched == null)
            {
                Logger.Error("AdminQuartzService.ExecuteAdminTasksAsync: Scheduler is null");
                return;
            }

            await sched.Start().ConfigureAwait(false);

            var runningCronJobs = await sched.GetCurrentlyExecutingJobs().ConfigureAwait(false);
            foreach (var runningCronJob in runningCronJobs)
            {
                try
                {
                    var jobId = runningCronJob.JobDetail.Key.Name.Replace("Name-", "").ToInt();
                    Logger.Info("RunningCron Job:" + runningCronJob.JobDetail.Key.Name + " " + runningCronJob.JobDetail.Key.Group + " JobId:" + jobId);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Error reading running cron job details");
                }
            }

            // Job 1: HelloJob (test / heartbeat)
            await ScheduleOrReschedule(sched, new ScheduleJob
            {
                JobId = 1,
                Group = "Admin",
                Name = "Testing",
                CronExp = "0 0/1 * * * ?",
                IsActive = true,
                TaskId = "Task-1"
            }, typeof(HelloJob)).ConfigureAwait(false);

            // Job 2: ClearExpiredShoppingCarts (every 3 days at 02:30)
            await ScheduleOrReschedule(sched, new ScheduleJob
            {
                JobId = 2,
                Group = "Admin",
                Name = "ClearExpiredShoppingCarts",
                CronExp = "0 30 2 */3 * ?",
                IsActive = true,
                TaskId = "Task-2"
            }, typeof(ClearExpiredShoppingCartsJob)).ConfigureAwait(false);

            // Job 3: ClearLogsFromDb (every 7 days at 02:30)
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
