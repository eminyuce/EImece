using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler.Jobs
{
    public class HelloJob : IJob
    {
        private readonly ILogger<HelloJob> _logger;

        public HelloJob(ILogger<HelloJob> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Execute(IJobExecutionContext context)
        {
            var jobId = context.JobDetail.JobDataMap.GetInt("EmailScheduleJob_JobId");
            var executionId = Guid.NewGuid().ToString("N");

            using (_logger.BeginScope(new { JobName = context.JobDetail.Key.Name, JobGroup = context.JobDetail.Key.Group, ExecutionId = executionId }))
            {
                _logger.LogInformation(
                    "HelloJob executing {JobId} on thread {ThreadId}",
                    jobId,
                    Thread.CurrentThread.ManagedThreadId);
            }

            return Task.CompletedTask;
        }
    }
}
