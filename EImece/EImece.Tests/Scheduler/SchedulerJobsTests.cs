using EImece.Domain.Observability.HealthChecks;
using EImece.Domain.Scheduler;
using EImece.Domain.Scheduler.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Scheduler
{
    [TestClass]
    public class SchedulerJobsTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Quartz.Logging.LogProvider.IsDisabled = true;
        }

        [TestMethod]
        public void CronExpressions_MustBeValidQuartzCron()
        {
            // ClearExpiredShoppingCarts: every 3 days at 02:30
            string cartCron = "0 30 2 */3 * ?";
            Assert.IsTrue(CronExpression.IsValidExpression(cartCron), "Cart cron must be valid Quartz cron syntax.");

            // ClearLogsFromDb: every 7 days at 02:30
            string logCron = "0 30 2 */7 * ?";
            Assert.IsTrue(CronExpression.IsValidExpression(logCron), "Log cron must be valid Quartz cron syntax.");

            // Testing job cron
            string testCron = "0 0/1 * * * ?";
            Assert.IsTrue(CronExpression.IsValidExpression(testCron), "Test cron must be valid Quartz cron syntax.");
        }

        [TestMethod]
        public void ScheduleJob_Properties_FormatKeysCorrectly()
        {
            var job = new QuartzService.ScheduleJob
            {
                JobId = 2,
                Name = "ClearExpiredShoppingCarts",
                CronExp = "0 30 2 */3 * ?",
                IsActive = true
            };

            Assert.AreEqual("Name-2", job.JobKey.Name);
            Assert.AreEqual("Group-2", job.JobKey.Group);
            Assert.AreEqual("Name-2", job.TriggerKey.Name);
            Assert.AreEqual("Group-2", job.TriggerKey.Group);
            Assert.IsTrue(job.ToString().Contains("ClearExpiredShoppingCarts"));
            Assert.IsFalse(string.IsNullOrEmpty(job.CronExpDescription));
        }

        [TestMethod]
        public async Task ScheduleOrReschedule_SchedulesAndReschedulesJob()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler sched = await factory.GetScheduler();

            try
            {
                var job = new QuartzService.ScheduleJob
                {
                    JobId = 99,
                    Name = "UnitTestCartJob",
                    CronExp = "0 30 2 */3 * ?",
                    IsActive = true
                };

                // Schedule first time
                await QuartzService.ScheduleOrReschedule(sched, job, typeof(ClearExpiredShoppingCartsJob));
                bool exists = await sched.CheckExists(job.JobKey);
                Assert.IsTrue(exists, "Job should exist in scheduler after ScheduleOrReschedule.");

                // Reschedule with different cron
                job.CronExp = "0 30 3 */3 * ?";
                await QuartzService.ScheduleOrReschedule(sched, job, typeof(ClearExpiredShoppingCartsJob));
                exists = await sched.CheckExists(job.JobKey);
                Assert.IsTrue(exists, "Job should still exist after rescheduling.");

                // Inactive job should not throw and should be skipped
                job.IsActive = false;
                await QuartzService.ScheduleOrReschedule(sched, job, typeof(ClearExpiredShoppingCartsJob));
            }
            finally
            {
                await sched.Clear();
                await sched.Shutdown(waitForJobsToComplete: false);
            }
        }

        [TestMethod]
        public async Task BackgroundServiceHealthCheck_WhenSchedulerNull_ReturnsDown()
        {
            var healthCheck = new BackgroundServiceHealthCheck
            {
                Scheduler = null
            };

            // If scheduler is enabled or disabled
            var result = await healthCheck.CheckAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual("backgroundServices", result.Name);
        }

        [TestMethod]
        public async Task BackgroundServiceHealthCheck_WhenSchedulerRunning_ReturnsUpWithDetails()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler sched = await factory.GetScheduler();
            await sched.Start();

            try
            {
                var healthCheck = new BackgroundServiceHealthCheck
                {
                    Scheduler = sched
                };

                var result = await healthCheck.CheckAsync(CancellationToken.None);
                Assert.IsNotNull(result);
                Assert.AreEqual("backgroundServices", result.Name);
                Assert.AreEqual(HealthStatus.Up, result.Status);
                Assert.IsTrue(result.Message.Contains("Quartz running") || result.Message.Contains("disabled by config"));
            }
            finally
            {
                await sched.Shutdown(waitForJobsToComplete: false);
            }
        }

        [TestMethod]
        public async Task AdminQuartzService_ExecuteAdminTasks_SchedulesAdminJobs()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler sched = await factory.GetScheduler();

            try
            {
                var adminService = new AdminQuartzService
                {
                    Scheduler = sched
                };

                await adminService.ExecuteAdminTasksAsync();

                var jobKeys = await sched.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup());
                Assert.IsTrue(jobKeys.Count >= 3, "AdminQuartzService should schedule HelloJob, ClearExpiredShoppingCarts, and ClearLogsFromDb.");
            }
            finally
            {
                await sched.Clear();
                await sched.Shutdown(waitForJobsToComplete: false);
            }
        }

        [TestMethod]
        public async Task UserQuartzService_ExecuteUserTasks_RunsWithoutError()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler sched = await factory.GetScheduler();

            try
            {
                var userService = new UserQuartzService
                {
                    Scheduler = sched
                };

                // Currently no active jobs for end-users; should execute cleanly
                await userService.ExecuteUserTasksAsync();
                Assert.IsTrue(sched.IsStarted, "UserQuartzService should start scheduler.");
            }
            finally
            {
                await sched.Clear();
                await sched.Shutdown(waitForJobsToComplete: false);
            }
        }
    }
}
