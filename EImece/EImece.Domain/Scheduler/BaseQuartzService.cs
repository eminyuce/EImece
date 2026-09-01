using CronExpressionDescriptor;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    public abstract class BaseQuartzService
    {
        protected readonly ILogger Logger;
        protected readonly IScheduler Scheduler;

        static BaseQuartzService()
        {
            Quartz.Logging.LogProvider.IsDisabled = true;
        }

        protected BaseQuartzService(IScheduler scheduler, ILogger logger)
        {
            Scheduler = scheduler;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task StartSchedulerServiceAsync();

        public async Task DeleteTask(int jobId, string group = "Default")
        {
            IScheduler sched = Scheduler;
            if (sched == null)
            {
                return;
            }

            var jobKey = new JobKey(string.Format("Name-{0}", jobId), string.Format("Group-{0}", group));
            try
            {
                bool isExists = await sched.CheckExists(jobKey).ConfigureAwait(false);
                if (isExists)
                {
                    bool result = await sched.Interrupt(jobKey).ConfigureAwait(false);
                    bool result2 = await sched.DeleteJob(jobKey).ConfigureAwait(false);
                    Logger.LogInformation(
                        "DeleteJob {JobName} {JobGroup} Interrupt={Interrupt} Deleted={Deleted}",
                        jobKey.Name,
                        jobKey.Group,
                        result,
                        result2);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting job {JobName} {JobGroup}", jobKey.Name, jobKey.Group);
            }
        }

        public async Task InterruptTask(int jobId, string group = "Default")
        {
            IScheduler sched = Scheduler;
            if (sched == null)
            {
                return;
            }

            var jobKey = new JobKey(string.Format("Name-{0}", jobId), string.Format("Group-{0}", group));
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                bool result = await sched.Interrupt(jobKey, cancellationTokenSource.Token).ConfigureAwait(false);
                Logger.LogTrace("Job interrupted {JobKey} Result={Result}", jobKey, result);
            }
        }

        public async Task ScheduleOrReschedule(IScheduler sched, ScheduleJob job, Type jobType)
        {
            if (sched == null || job == null || jobType == null)
            {
                return;
            }

            if (!job.IsActive)
            {
                Logger.LogInformation("Skipping inactive job {JobName}", job.Name);
                return;
            }

            var jobKey = job.JobKey;
            var triggerKey = job.TriggerKey;
            bool isExists = await sched.CheckExists(jobKey).ConfigureAwait(false);

            IJobDetail cronJob = JobBuilder.Create(jobType)
                .UsingJobData("EmailScheduleJob_JobId", job.JobId)
                .WithIdentity(jobKey)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .ForJob(cronJob)
                .WithCronSchedule(job.CronExp)
                .WithIdentity(triggerKey)
                .Build();

            if (isExists)
            {
                Logger.LogInformation("RescheduleJob {Job}", job);
                await sched.RescheduleJob(triggerKey, trigger).ConfigureAwait(false);
            }
            else
            {
                Logger.LogInformation("ScheduleJob {Job}", job);
                await sched.ScheduleJob(cronJob, trigger).ConfigureAwait(false);
            }
        }

        public class ScheduleJob
        {
            public int JobId { get; set; }
            public string CronExp { get; set; }
            public string Group { get; set; }

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
                return string.Format("JobId:{0}, group:{1}, name:{2}, cron:{3}", JobId, Group ?? JobId.ToString(), Name, CronExp);
            }

            public static ScheduleJob CreateTest()
            {
                return new ScheduleJob() { JobId = 1, Group = "Admin", CronExp = "0 0/1 * * * ?", Name = "Testing", IsActive = true, TaskId = "Task-1" };
            }

            public JobKey JobKey
            {
                get
                {
                    var groupName = !string.IsNullOrEmpty(Group) ? Group : JobId.ToString();
                    return new JobKey(string.Format("Name-{0}", JobId), string.Format("Group-{0}", groupName));
                }
            }

            public TriggerKey TriggerKey
            {
                get
                {
                    var groupName = !string.IsNullOrEmpty(Group) ? Group : JobId.ToString();
                    return new TriggerKey(string.Format("Name-{0}", JobId), string.Format("Group-{0}", groupName));
                }
            }
        }
    }
}
