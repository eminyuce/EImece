using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Quartz;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Scheduler.Jobs
{
    public class ClearExpiredShoppingCartsJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            var sw = Stopwatch.StartNew();
            var jobKey = context?.JobDetail?.Key;
            Logger.Info("ClearExpiredShoppingCartsJob started. JobKey: {0}", jobKey);

            try
            {
                var provider = DomainServiceProvider.Instance;
                if (provider == null)
                {
                    Logger.Warn("ClearExpiredShoppingCartsJob skipped: DI ServiceProvider is null.");
                    return;
                }

                var expirationDays = AppConfig.GetConfigInt("ShoppingCart_Expiration_Days", 30);
                if (expirationDays <= 0)
                {
                    expirationDays = 30;
                }

                int deletedCount = 0;
                using (var scope = provider.CreateScope())
                {
                    var cartService = scope.ServiceProvider.GetService<IShoppingCartService>();
                    if (cartService == null)
                    {
                        Logger.Error("ClearExpiredShoppingCartsJob: IShoppingCartService could not be resolved from scope.");
                        return;
                    }

                    var ct = context != null ? context.CancellationToken : CancellationToken.None;
                    deletedCount = await cartService.ClearExpiredShoppingCartsAsync(expirationDays, ct).ConfigureAwait(false);
                }

                sw.Stop();
                Logger.Info("ClearExpiredShoppingCartsJob finished successfully in {0} ms. Total expired carts cleaned: {1}",
                    sw.ElapsedMilliseconds, deletedCount);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Logger.Error(ex, "ClearExpiredShoppingCartsJob encountered an error after {0} ms: {1}",
                    sw.ElapsedMilliseconds, ex.Message);
            }
        }
    }
}
