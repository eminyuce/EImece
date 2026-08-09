using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EImece.App_Start
{
    /// <summary>
    /// Runs the (expensive) post-clear cache warm-up off the request thread so the admin
    /// "Clear Cache" action can return immediately. The work is registered with the ASP.NET
    /// runtime via <see cref="HostingEnvironment.QueueBackgroundWorkItem(Func{CancellationToken, Task})"/>
    /// (which delays app-pool shutdown up to ~90s so warm-up isn't lost on recycle) and executes
    /// inside its own DI scope with a fresh DbContext — the request-scoped services/DbContext of
    /// the originating request are already disposed by the time this runs. Each major step is
    /// timed and logged so slow operations can be identified from the logs.
    /// </summary>
    public static class CacheWarmUpJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // 0 = idle, 1 = a warm-up is currently in progress. Prevents overlapping (stacking)
        // warm-ups when the admin clicks Refresh repeatedly, which protects the DB and server.
        private static int _running;

        /// <summary>
        /// Queues a background warm-up. Request-bound values (base URL, language) must be captured
        /// by the caller and passed in, because <see cref="System.Web.HttpContext.Current"/> is not
        /// available on the background thread.
        /// </summary>
        public static void Queue(string baseUrl, int language)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                Logger.Info("Cache warm-up already in progress; skipping duplicate request.");
                return;
            }

            try
            {
                if (HostingEnvironment.IsHosted)
                {
                    HostingEnvironment.QueueBackgroundWorkItem(ct => RunAsync(baseUrl, language, ct));
                }
                else
                {
                    // Non-hosted context (unit tests / console): run detached so callers don't block.
                    Task.Run(() => RunAsync(baseUrl, language, CancellationToken.None));
                }
            }
            catch (Exception ex)
            {
                // Queueing must never break the user's request; release the gate if it failed.
                Interlocked.Exchange(ref _running, 0);
                Logger.Error(ex, "Failed to queue cache warm-up.");
            }
        }

        private static async Task RunAsync(string baseUrl, int language, CancellationToken cancellationToken)
        {
            var total = Stopwatch.StartNew();
            try
            {
                var provider = DependencyInjectionConfig.ServiceProvider;
                if (provider == null)
                {
                    Logger.Warn("Cache warm-up skipped: DI ServiceProvider is not initialised.");
                    return;
                }

                using (var scope = provider.CreateScope())
                {
                    var sp = scope.ServiceProvider;

                    var faqService = sp.GetRequiredService<IFaqService>();
                    var settingService = sp.GetRequiredService<ISettingService>();
                    var siteMapService = sp.GetRequiredService<SiteMapService>();
                    var mainPageImageService = sp.GetRequiredService<IMainPageImageService>();
                    var productCategoryService = sp.GetRequiredService<IProductCategoryService>();
                    var menuService = sp.GetRequiredService<IMenuService>();
                    var productService = sp.GetRequiredService<IProductService>();
                    var mailTemplateService = sp.GetRequiredService<IMailTemplateService>();
                    var imageDownloadService = sp.GetRequiredService<IImageDownloadService>();

                    // NOTE: EF6 DbContext is not thread-safe, and all services in this scope share one
                    // DbContext, so the DB-priming steps run sequentially. The genuinely expensive,
                    // network-bound sitemap crawl (below) is what gets parallelised.
                    Measure("Faq", () => faqService.GetActiveBaseEntitiesFromCache(true, language));
                    Measure("EmailAccount", () => settingService.GetEmailAccount());
                    Measure("AllActiveSettings", () => settingService.GetAllActiveSettings());
                    Measure("GenerateSiteMap", () => siteMapService.GenerateSiteMap());
                    Measure("MainPageViewModel", () => mainPageImageService.GetMainPageViewModel(language));
                    Measure("FooterViewModel", () => mainPageImageService.GetFooterViewModel(language));
                    Measure("ProductCategories", () =>
                    {
                        var activeCategories = productCategoryService.GetActiveBaseContentsFromCache(true, language);
                        if (activeCategories.IsNotEmpty())
                        {
                            foreach (var c in activeCategories.Take(10))
                            {
                                productCategoryService.GetProductCategoryViewModel(c.Id);
                            }
                        }
                    });
                    Measure("Menus", () =>
                    {
                        menuService.GetMenus();
                        menuService.BuildTree(true, language);
                    });
                    Measure("ProductCategoryTrees", () =>
                    {
                        productCategoryService.BuildTree(true, language);
                        productCategoryService.BuildNavigation(true, language);
                    });
                    Measure("MenuContents", () => menuService.GetActiveBaseContentsFromCache(true, language));

                    List<Product> products = null;
                    Measure("Products", () => products = productService.GetActiveBaseContentsFromCache(true, language));
                    // Prime the hierarchical product:list:* MemoryCache keys used by the storefront
                    // (GetActiveProducts / GetMainPageProducts) after Admin Refresh cleared them.
                    Measure("ActiveProductsList", () => productService.GetActiveProducts(language));
                    Measure("MainPageProducts", () => productService.GetMainPageProducts(1, language));
                    Measure("MainPageProductCategories", () => productCategoryService.GetMainPageProductCategories(language));
                    Measure("MailTemplates", () => mailTemplateService.GetAllMailTemplatesWithCache());
                    Measure("ProductDetails", () =>
                    {
                        if (products.IsNotEmpty())
                        {
                            foreach (var p in products.Take(10))
                            {
                                productService.GetProductDetailViewModelById(p.Id);
                            }
                        }
                    });

                    // Output-cache warm-up: fetch the freshly generated sitemap, then request every
                    // URL it contains (the service now crawls those URLs in parallel).
                    var sitemapSw = Stopwatch.StartNew();
                    try
                    {
                        var buffer = await imageDownloadService
                            .GetImageAsync(baseUrl + "/sitemap.xml", null, cancellationToken)
                            .ConfigureAwait(false);
                        if (buffer != null && buffer.Length > 0)
                        {
                            var sitemapXml = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                            await siteMapService
                                .ReadSiteMapXmlAndRequestAsync(sitemapXml, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        Logger.Info("Cache warm-up step 'SitemapCrawl' finished in {0} ms", sitemapSw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Cache warm-up step 'SitemapCrawl' failed after {0} ms", sitemapSw.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cache warm-up failed.");
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
                Logger.Info("Cache warm-up completed in {0} ms", total.ElapsedMilliseconds);
            }
        }

        private static void Measure(string step, Action action)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                action();
                Logger.Info("Cache warm-up step '{0}' finished in {1} ms", step, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                // Resilient per-step: a failing warm-up step is logged but does not abort the rest.
                Logger.Error(ex, "Cache warm-up step '{0}' failed after {1} ms", step, sw.ElapsedMilliseconds);
            }
        }
    }
}
