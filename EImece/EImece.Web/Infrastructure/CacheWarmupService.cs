using EImece.Domain;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EImece.Web.Infrastructure
{
    public static class CacheWarmupService
    {
        public static void StartWarmup()
        {
            if (!HostingEnvironment.IsHosted)
            {
                return;
            }

            HostingEnvironment.QueueBackgroundWorkItem(async cancellationToken =>
            {
                var logger = DomainServiceProvider.GetService<ILogger<HostingEnvironmentBackgroundWorkQueue>>();
                try
                {
                    // Allow server initialization to complete
                    await Task.Delay(3000, cancellationToken).ConfigureAwait(false);
                    if (cancellationToken.IsCancellationRequested) return;

                    logger?.LogInformation("CacheWarmupService: Starting background storefront cache pre-warming...");

                    // 1. Seed Domain/Application MemoryCache
                    try
                    {
                        var categoryService = DomainServiceProvider.GetService<IProductCategoryService>();
                        if (categoryService != null)
                        {
                            await categoryService.BuildTreeAsync(true, 1).ConfigureAwait(false);
                            await categoryService.BuildStorefrontNavigationTreeAsync(1, cancellationToken).ConfigureAwait(false);
                            await categoryService.GetStorefrontMainPageCategoriesAsync(1, cancellationToken).ConfigureAwait(false);
                        }

                        var menuService = DomainServiceProvider.GetService<IMenuService>();
                        if (menuService != null)
                        {
                            await menuService.GetStorefrontMenuNavigationAsync(1, cancellationToken).ConfigureAwait(false);
                            await menuService.GetStorefrontActiveMenusCachedAsync(1).ConfigureAwait(false);
                        }

                        var productService = DomainServiceProvider.GetService<IProductService>();
                        if (productService != null)
                        {
                            await productService.GetMainPageProductsAsync(1, 1, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "CacheWarmupService: Domain memory cache seeding error (non-fatal).");
                    }

                    // 2. Pre-warm HTTP OutputCache via internal loopback HTTP requests
                    try
                    {
                        var protocol = AppConfig.HttpProtocol ?? "http";
                        var domain = AppConfig.Domain;
                        var baseUri = !string.IsNullOrEmpty(domain) ? $"{protocol}://{domain.TrimEnd('/')}" : "http://127.0.0.1";

                        using (var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = true })
                        using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) })
                        {
                            client.DefaultRequestHeaders.Add("User-Agent", "EImece-Warmup/1.0");

                            var warmupRoutes = new[]
                            {
                                "/",
                                "/info/aboutus",
                                "/info/deliveryinfo"
                            };

                            foreach (var route in warmupRoutes)
                            {
                                if (cancellationToken.IsCancellationRequested) break;
                                try
                                {
                                    var uri = new Uri($"{baseUri}{route}");
                                    using (var resp = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false))
                                    {
                                        logger?.LogDebug("CacheWarmupService: Pre-warmed route {0} -> HTTP {1}", route, (int)resp.StatusCode);
                                    }
                                }
                                catch
                                {
                                    // Non-fatal if loopback endpoint differs in test environments
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "CacheWarmupService: Loopback HTTP warmup error (non-fatal).");
                    }

                    logger?.LogInformation("CacheWarmupService: Background cache pre-warming finished successfully.");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "CacheWarmupService encountered an unexpected error during warmup.");
                }
            });
        }
    }
}
