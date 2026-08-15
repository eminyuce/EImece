using System;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler
{
    /// <summary>
    /// Backward-compatible alias/wrapper inheriting from <see cref="AdminQuartzService"/>.
    /// For new administrative jobs, inject <see cref="AdminQuartzService"/>.
    /// For customer / storefront scheduled jobs, inject <see cref="UserQuartzService"/>.
    /// </summary>
    public class QuartzService : AdminQuartzService
    {
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