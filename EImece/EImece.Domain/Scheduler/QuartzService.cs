using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    /// <summary>
    /// Backward-compatible alias/wrapper inheriting from <see cref="AdminQuartzService"/>.
    /// </summary>
    public class QuartzService : AdminQuartzService
    {
        public QuartzService(IScheduler scheduler, ILogger<QuartzService> logger) : base(scheduler, logger)
        {
        }
        public async Task ExecuteMultiplyTask()
        {
            await ExecuteAdminTasksAsync().ConfigureAwait(false);
        }

        public async Task DeleteNonProcessingTask(int jobId)
        {
            await DeleteTask(jobId, "Admin").ConfigureAwait(false);
        }
    }
}