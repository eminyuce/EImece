using CronExpressionDescriptor;
using EImece.Domain.Helpers;
using EImece.Domain.Scheduler.Jobs;
using Ninject;
using NLog;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    public class QuartzService
    {
        // https://www.freeformatter.com/cron-expression-generator-quartz.html
        //https://cronexpressiondescriptor.azurewebsites.net
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public Task<IScheduler> Scheduler { get; set; }

        public void StartSchedulerService()
        {
            Logger.Info("StartSchedulerService has started");

            // We do not want to trigger cron job for development environment.
            var quartzEnabled = AppConfig.GetConfigBool("Quartz_Scheduler_IsEnabled", true);
            if (quartzEnabled == false)
            {
                return;
            }

            try
            {
                Logger.Info("ExecuteMultiplyTask Cron has started");
                ExecuteMultiplyTask().Wait();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public async Task DeleteNonProcessingTask(int jobId)
        {
            // get a scheduler
            IScheduler sched = await Scheduler;
            //  await sched.Clear();
            var job = ScheduleJob.CreateTest();
            try
            {
                var jobKey = job.JobKey;
                var triggerKey = job.TriggerKey;
                bool isExists = await sched.CheckExists(jobKey);
                if (isExists)
                {
                    bool result = await sched.Interrupt(jobKey);
                    bool result2 = await sched.DeleteJob(jobKey);
                    Logger.Info("DeleteJob Job:" + jobKey.Name + " " + jobKey.Group + " Interrupt Result:" + result + " DeleteJob result:" + result2);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public async Task InterruptTask(int jobId)
        {
            var job = ScheduleJob.CreateTest();
            var jobKey = job.JobKey;
            // get a scheduler
            IScheduler sched = await Scheduler;
            var cancellationTokenSource = new CancellationTokenSource();
            bool result = await sched.Interrupt(jobKey, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
            Logger.Trace("Job is Interrupted jobKey:" + jobKey + " Interrupt Result:" + result);
        }

        public async Task ExecuteMultiplyTask()
        {
            // get a scheduler
            IScheduler sched = await Scheduler;
            // and start it off
            await sched.Start();

            var runningCronJobs = await sched.GetCurrentlyExecutingJobs();
            foreach (var runningCronJob in runningCronJobs)
            {
                try
                {
                    var jobId = runningCronJob.JobDetail.Key.Name.Replace("Name-", "").ToInt();
                    Logger.Info("RunningCron Job:" + runningCronJob.JobDetail.Key.Name + " " + runningCronJob.JobDetail.Key.Group + " JobId:" + jobId);
                }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                {
                }
                //  jobsFromDb.Contains()
            }

            var job = ScheduleJob.CreateTest();
            var jobKey = job.JobKey;
            var triggerKey = job.TriggerKey;
            bool isExists = await sched.CheckExists(jobKey);

            ITrigger trigger = null;

            // define the job and tie it to our DumbJob class
            IJobDetail cronJob = JobBuilder.Create<HelloJob>()
                .UsingJobData("EmailScheduleJob_JobId", job.JobId)
                .WithIdentity(jobKey)
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            trigger = TriggerBuilder.Create()
                                       .ForJob(cronJob)
                                       .WithCronSchedule(job.CronExp).StartNow()
                                       .WithIdentity(triggerKey)
                                       .Build();

            if (isExists)
            {
                Logger.Info("RescheduleJob:" + job.ToString());
                await sched.RescheduleJob(triggerKey, trigger);
            }
            else
            {
                Logger.Info("ScheduleJob:" + job.ToString());
                await sched.ScheduleJob(cronJob, trigger);
            }
        }

        public class ScheduleJob
        {
            public int JobId { get; set; }
            public string CronExp { get; set; }

            public string CronExpDescription
            {
                get
                {
                    var options = new Options
                    {
                        Locale = "en"
                    };
                    return ExpressionDescriptor.GetDescription(CronExp, options);
                }
            }

            public string Name { get; set; }
            public bool IsActive { get; set; }
            public string TaskId { get; set; }

            public override string ToString()
            {
                return string.Format("JobId:{0}, name:{1}, cron:{2}", JobId, Name, CronExp);
            }

            public static ScheduleJob CreateTest()
            {
                return new ScheduleJob() { JobId = 1, CronExp = "* * 0/24 ? * * *", Name = "Testing", IsActive = true, TaskId = "Task-1" };
            }

            public JobKey JobKey
            {
                get
                {
                    return new JobKey(string.Format("Name-{0}", JobId), string.Format("Group-{0}", JobId));
                }
            }

            public TriggerKey TriggerKey
            {
                get
                {
                    return new TriggerKey(string.Format("Name-{0}", JobId), string.Format("Group-{0}", JobId));
                }
            }
        }
    }
}