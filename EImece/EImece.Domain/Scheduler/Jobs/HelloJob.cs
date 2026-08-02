using NLog;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler.Jobs
{
    public class HelloJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int JobId = dataMap.GetInt("EmailScheduleJob_JobId");

            Logger.Info("WriteLog Executing for " + JobId + " CurrentThread: " + Thread.CurrentThread.ManagedThreadId);
            // FIX: removed Task.Run for trivial synchronous work — it added pointless thread-pool
            // scheduling overhead. Quartz already invokes Execute on a worker thread.
            Console.WriteLine("Test");
            await Task.CompletedTask;
        }
    }
}