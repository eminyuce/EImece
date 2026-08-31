using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler.Jobs
{
    public class ClearLogsFromDbJob : IJob
    {
        private readonly ILogger<ClearLogsFromDbJob> _logger;

        public ClearLogsFromDbJob(ILogger<ClearLogsFromDbJob> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sw = Stopwatch.StartNew();
            var jobKey = context?.JobDetail?.Key;
            var executionId = Guid.NewGuid().ToString("N");

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["JobName"] = jobKey?.Name,
                ["JobGroup"] = jobKey?.Group,
                ["ExecutionId"] = executionId,
            }))
            {
                _logger.LogDebug("ClearLogsFromDbJob started");

                try
                {
                    var provider = DomainServiceProvider.Instance;
                    if (provider == null)
                    {
                        _logger.LogWarning("ClearLogsFromDbJob skipped: DI ServiceProvider is null.");
                        return;
                    }

                    var retentionDays = AppConfig.GetConfigInt("AppLog_Retention_Days", 90);
                    if (retentionDays <= 0)
                    {
                        retentionDays = 90;
                    }

                    var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                    int deletedCount;

                    using (var scope = provider.CreateScope())
                    {
                        var logRepo = scope.ServiceProvider.GetRequiredService<AppLogRepository>();
                        var ct = context?.CancellationToken ?? CancellationToken.None;
                        deletedCount = await logRepo.DeleteOldLogsAsync(cutoffDate, 1000, ct).ConfigureAwait(false);
                    }

                    sw.Stop();
                    _logger.LogInformation(
                        "ClearLogsFromDbJob finished {ElapsedMs} ms deleted={DeletedCount} cutoff={CutoffDate:yyyy-MM-dd}",
                        sw.ElapsedMilliseconds,
                        deletedCount,
                        cutoffDate);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "ClearLogsFromDbJob failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
                }
            }
        }
    }
}
