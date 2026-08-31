using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;
using System;

namespace EImece.Domain.Scheduler
{
    /// <summary>
    /// Resolves Quartz jobs from Microsoft.Extensions.DependencyInjection so jobs receive ILogger&lt;T&gt; via constructor injection.
    /// </summary>
    public sealed class MsDiJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MsDiJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle?.JobDetail?.JobType == null)
            {
                throw new SchedulerException("Job type is required.");
            }

            return (IJob)_serviceProvider.GetRequiredService(bundle.JobDetail.JobType);
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
