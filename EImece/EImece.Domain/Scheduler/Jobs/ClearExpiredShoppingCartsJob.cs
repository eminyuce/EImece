using EImece.Domain.DependencyInjection;
using EImece.Domain.Services.IServices;
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
    public class ClearExpiredShoppingCartsJob : IJob
    {
        private readonly ILogger<ClearExpiredShoppingCartsJob> _logger;

        public ClearExpiredShoppingCartsJob(ILogger<ClearExpiredShoppingCartsJob> logger)
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
                _logger.LogDebug("ClearExpiredShoppingCartsJob started");

                try
                {
                    var provider = DomainServiceProvider.Instance;
                    if (provider == null)
                    {
                        _logger.LogWarning("ClearExpiredShoppingCartsJob skipped: DI ServiceProvider is null.");
                        return;
                    }

                    var expirationDays = AppConfig.GetConfigInt("ShoppingCart_Expiration_Days", 30);
                    if (expirationDays <= 0)
                    {
                        expirationDays = 30;
                    }

                    int deletedCount;
                    using (var scope = provider.CreateScope())
                    {
                        var cartService = scope.ServiceProvider.GetRequiredService<IShoppingCartService>();
                        var ct = context?.CancellationToken ?? CancellationToken.None;
                        deletedCount = await cartService.ClearExpiredShoppingCartsAsync(expirationDays, ct).ConfigureAwait(false);
                    }

                    sw.Stop();
                    _logger.LogInformation(
                        "ClearExpiredShoppingCartsJob finished {ElapsedMs} ms deleted={DeletedCount}",
                        sw.ElapsedMilliseconds,
                        deletedCount);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "ClearExpiredShoppingCartsJob failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
                }
            }
        }
    }
}
