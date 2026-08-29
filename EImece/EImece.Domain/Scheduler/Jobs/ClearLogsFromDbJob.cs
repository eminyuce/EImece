using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Quartz;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler.Jobs
{
    public class ClearLogsFromDbJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            var sw = Stopwatch.StartNew();
            var jobKey = context?.JobDetail?.Key;
            var correlationId = $"job-log-cleanup-{Guid.NewGuid():N}";
            using (ScopeContext.PushProperty("CorrelationId", correlationId))
            {
                Logger.Debug("ClearLogsFromDbJob started. JobKey: {0} (CorrelationId: {1})", jobKey, correlationId);

                try
                {
                    var provider = DomainServiceProvider.Instance;
                    if (provider == null)
                    {
                        Logger.Warn("ClearLogsFromDbJob skipped: DI ServiceProvider is null.");
                        return;
                    }

                    var retentionDays = AppConfig.GetConfigInt("AppLog_Retention_Days", 90);
                    if (retentionDays <= 0)
                    {
                        retentionDays = 90;
                    }

                    var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                    int deletedCount = 0;

                    using (var scope = provider.CreateScope())
                    {
                        var logRepo = scope.ServiceProvider.GetService<AppLogRepository>();
                        if (logRepo == null)
                        {
                            Logger.Error("ClearLogsFromDbJob: AppLogRepository could not be resolved from scope.");
                            return;
                        }

                        var ct = context != null ? context.CancellationToken : CancellationToken.None;
                        deletedCount = await logRepo.DeleteOldLogsAsync(cutoffDate, 1000, ct).ConfigureAwait(false);
                    }

                    sw.Stop();
                    Logger.Info("ClearLogsFromDbJob finished successfully in {0} ms. Total log records deleted: {1} (cutoff date: {2:yyyy-MM-dd HH:mm:ss})",
                        sw.ElapsedMilliseconds, deletedCount, cutoffDate);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    Logger.Error(ex, "ClearLogsFromDbJob encountered an error after {0} ms: {1}",
                        sw.ElapsedMilliseconds, ex.Message);
                }
            }
        }
    }
}
