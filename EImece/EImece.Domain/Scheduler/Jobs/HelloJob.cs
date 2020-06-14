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
            await Task.Run(() =>
            {
                Console.WriteLine("Test");
            });
        }
    }
}