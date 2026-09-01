using EImece.Areas.Admin.Models;
using EImece.Domain;
using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using EImece.Web.Areas.Admin.Controllers;
using Griddly.Mvc.Results;
using Microsoft.Extensions.Logging;
using Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    /// <summary>
    /// Dedicated cache administration: maintenance operations plus read-only diagnostics.
    /// Authorized via <see cref="BaseAdminController"/> (Administrator + Editor).
    /// </summary>
    public class CacheController : BaseAdminController
    {
        private const string AdminAreaName = "admin";
        private const int DefaultPageSize = 50;

        private readonly IProductService _productService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IEimeceCacheProvider _cache;
        private readonly IHttpRuntimeCacheClearer _httpRuntimeCacheClearer;

        public CacheController(
            ISettingService settingService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            IEimeceCacheProvider cache,
            IHttpRuntimeCacheClearer httpRuntimeCacheClearer,
            ILogger<CacheController> logger)
            : base(settingService, logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _productCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpRuntimeCacheClearer = httpRuntimeCacheClearer;
        }

        [HttpGet]
        public ActionResult Index(string search = "", string category = "all", string status = "all", int page = 1)
        {
            ViewBag.Title = AdminResource.CacheAdministration;
            var model = BuildViewModel(search, category, status, page);
            return View(model);
        }

        /// <summary>
        /// AJAX refresh of diagnostics metadata and statistics. Does not modify the cache.
        /// </summary>
        [HttpGet]
        public ActionResult Diagnostics(string search = "", string category = "all", string status = "all", int page = 1)
        {
            var model = BuildViewModel(search, category, status, page);
            return Json(ToDiagnosticsPayload(model), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult IndexGrid(string search = "", string category = "all", string status = "all")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search, category, status });
            }

            var entries = CacheDiagnostics.GetMatchingEntries(search, category, status);
            return new QueryableResult<CacheEntrySnapshot>(entries.AsQueryable());
        }

        /// <summary>
        /// Metadata for a single cache entry. Never returns the cached value.
        /// </summary>
        [HttpGet]
        public ActionResult Entry(string key)
        {
            var snapshot = CacheDiagnostics.GetEntry(key);
            if (snapshot == null)
            {
                return HttpNotFound();
            }

            return Json(ToEntryPayload(snapshot), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Excel or CSV of cache health/performance. Exports matching keys (not paged).
        /// Never includes cached values.
        /// </summary>
        [HttpGet]
        [ActionName("ExportExcel")]
        public ActionResult ExportExcel(string format = "excel", string search = "", string category = "all", string status = "all")
        {
            var metrics = CacheDiagnostics.GetMetrics();
            var entries = CacheDiagnostics.GetMatchingEntries(search, category, status);
            var exportedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var fileName = "cache-diagnostics";

            var summaryRows = new[]
            {
                new { Metric = "ExportedAt", Value = exportedAt },
                new { Metric = "IsCacheActive", Value = AppConfig.IsCacheActive.ToString() },
                new { Metric = "TotalReads", Value = metrics.TotalReads.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "Hits", Value = metrics.Hits.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "Misses", Value = metrics.Misses.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "HitRatioPercent", Value = metrics.HitRatioPercent.ToString("0.00", CultureInfo.InvariantCulture) },
                new { Metric = "Sets", Value = metrics.Sets.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "Removals", Value = metrics.Removals.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "Expirations", Value = metrics.Expirations.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "OutputHits", Value = metrics.OutputHits.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "OutputMisses", Value = metrics.OutputMisses.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "OutputHitRatioPercent", Value = metrics.OutputHitRatioPercent.ToString("0.00", CultureInfo.InvariantCulture) },
                new { Metric = "AvgCachedMs", Value = FormatOptionalMs(metrics.AvgCachedMs) },
                new { Metric = "AvgUncachedMs", Value = FormatOptionalMs(metrics.AvgUncachedMs) },
                new { Metric = "OutputAvgCachedMs", Value = FormatOptionalMs(metrics.OutputAvgCachedMs) },
                new { Metric = "OutputAvgUncachedMs", Value = FormatOptionalMs(metrics.OutputAvgUncachedMs) },
                new { Metric = "TrackedEntryCount", Value = metrics.TrackedEntryCount.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "ExportedEntryCount", Value = entries.Count.ToString("D", CultureInfo.InvariantCulture) },
                new { Metric = "Search", Value = search ?? "" },
                new { Metric = "CategoryFilter", Value = string.IsNullOrWhiteSpace(category) ? "all" : category },
                new { Metric = "StatusFilter", Value = string.IsNullOrWhiteSpace(status) ? "all" : status }
            };

            var categoryRows = entries
                .GroupBy(e => e.Category ?? "Other", StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Category = g.Key,
                    Entries = g.Count(),
                    Active = g.Count(e => string.Equals(e.Status, CacheDiagnostics.StatusActive, StringComparison.OrdinalIgnoreCase)),
                    Expired = g.Count(e => string.Equals(e.Status, CacheDiagnostics.StatusExpired, StringComparison.OrdinalIgnoreCase)),
                    Healthy = g.Count(e => ResolveHealth(e) == "Healthy"),
                    Cold = g.Count(e => ResolveHealth(e) == "Cold"),
                    TotalHits = g.Sum(e => e.HitCount)
                })
                .ToList();

            var entryRows = entries.Select(ToExportRow).ToList();

            var isCsv = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
            if (isCsv)
            {
                var csvTable = EnsureColumns(
                    GeneralHelper.LINQToDataTable(entryRows),
                    () => GeneralHelper.LINQToDataTable(new[] { ToExportRow(new CacheEntrySnapshot()) }));
                csvTable.TableName = fileName;
                return DownloadFileDataTable(csvTable, fileName, "csv");
            }

            var tables = new System.Collections.Generic.List<System.Data.DataTable>
            {
                NamedTable(GeneralHelper.LINQToDataTable(summaryRows), "Summary"),
                NamedTable(
                    EnsureColumns(
                        GeneralHelper.LINQToDataTable(categoryRows),
                        () => GeneralHelper.LINQToDataTable(new[] { new { Category = "", Entries = 0, Active = 0, Expired = 0, Healthy = 0, Cold = 0, TotalHits = 0L } })),
                    "Categories"),
                NamedTable(EnsureColumns(GeneralHelper.LINQToDataTable(entryRows), () => GeneralHelper.LINQToDataTable(new[] { ToExportRow(new CacheEntrySnapshot()) })), "Entries")
            };

            var bytes = ExcelHelper.GetExcelByteArrayFromDataTable(tables);
            var stamped = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", fileName, DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            return File(bytes, "application/vnd.ms-excel", stamped + ".xls");
        }

        [HttpGet]
        public ActionResult ClearCache()
        {
            var evictionSw = System.Diagnostics.Stopwatch.StartNew();
            var dataKeysRemoved = AdminCacheMaintenance.ClearAllData(SettingService, _productService, _cache);
            Logger.LogInformation(
                "ClearCache: eviction completed in {0} ms (provider data keys removed: {1})",
                evictionSw.ElapsedMilliseconds,
                dataKeysRemoved);

            var baseUrl = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
            App_Start.CacheWarmUpJob.Queue(baseUrl, CurrentLanguage);

            string redirectUrl;
            if (!SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                redirectUrl = Url.Action("Index", "Cache", new { area = AdminAreaName });
            }

            redirectUrl = NormalizeClearCacheReturnUrl(redirectUrl);

            ViewBag.Title = AdminResource.Refresh;
            ViewBag.ReturnUrl = redirectUrl;
            return View("~/Areas/Admin/Views/Dashboard/ClearCache.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvalidateCache(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool fullWipe;
            var removed = AdminCacheMaintenance.Invalidate(
                target,
                SettingService,
                _productService,
                _productCategoryService,
                _cache,
                _httpRuntimeCacheClearer,
                out fullWipe);

            if (removed < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Logger.LogInformation(
                "InvalidateCache target={0} removed={1} in {2} ms (fullWipe={3}) by {4}",
                target,
                removed,
                sw.ElapsedMilliseconds,
                fullWipe,
                User?.Identity?.Name ?? "unknown");

            if (fullWipe)
            {
                var baseUrl = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
                App_Start.CacheWarmUpJob.Queue(baseUrl, CurrentLanguage);
            }

            string redirectUrl;
            if (!SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                redirectUrl = Url.Action("Index", "Cache", new { area = AdminAreaName });
            }

            SetSuccessMessage(string.Format(
                AdminResource.CacheInvalidatedFormat,
                System.Web.HttpUtility.HtmlEncode(target)));

            return Redirect(redirectUrl);
        }

        private CacheAdminViewModel BuildViewModel(string search, string category, string status, int page)
        {
            var query = CacheDiagnostics.QueryEntries(search, category, status, page, DefaultPageSize);
            return new CacheAdminViewModel
            {
                Metrics = CacheDiagnostics.GetMetrics(),
                Overview = CacheDiagnostics.BuildOverview(),
                Entries = query.Entries,
                Categories = query.Categories,
                Search = search ?? "",
                Category = string.IsNullOrWhiteSpace(category) ? "all" : category,
                Status = string.IsNullOrWhiteSpace(status) ? "all" : status,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = query.TotalCount,
                GeneratedAt = DateTimeOffset.Now,
                IsCacheActive = AppConfig.IsCacheActive
            };
        }

        private static object ToDiagnosticsPayload(CacheAdminViewModel model)
        {
            var overview = model.Overview ?? CacheDiagnostics.BuildOverview();
            return new
            {
                success = true,
                generatedAt = model.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                isCacheActive = model.IsCacheActive,
                overview = new
                {
                    combined = ToLayerPayload(overview.Combined),
                    page = ToLayerPayload(overview.Page),
                    data = ToLayerPayload(overview.Data)
                },
                stats = new
                {
                    totalReads = model.Metrics.TotalReads,
                    hits = model.Metrics.Hits,
                    misses = model.Metrics.Misses,
                    hitRatio = model.Metrics.HitRatioPercent,
                    sets = model.Metrics.Sets,
                    removals = model.Metrics.Removals,
                    expirations = model.Metrics.Expirations,
                    entryCount = model.Metrics.TrackedEntryCount
                },
                search = model.Search,
                category = model.Category,
                status = model.Status,
                page = model.Page,
                pageSize = model.PageSize,
                totalCount = model.TotalCount,
                totalPages = model.TotalPages,
                categories = model.Categories,
                entries = model.Entries.Select(ToEntryPayload).ToList()
            };
        }

        private static object ToLayerPayload(CacheLayerSnapshot layer)
        {
            layer = layer ?? new CacheLayerSnapshot();
            var cannot = AdminResource.CachePerformanceNotMeasured;
            var cachedBar = 0d;
            if (layer.HasTiming && layer.AvgUncachedMs.HasValue && layer.AvgUncachedMs.Value > 0 && layer.AvgCachedMs.HasValue)
            {
                cachedBar = Math.Max(2d, Math.Min(100d, layer.AvgCachedMs.Value / layer.AvgUncachedMs.Value * 100d));
            }

            return new
            {
                effectiveness = layer.Effectiveness.ToString(),
                hits = layer.Hits,
                misses = layer.Misses,
                totalReads = layer.TotalReads,
                hitRatio = layer.HitRatioPercent,
                activeEntries = layer.ActiveEntries,
                hasTiming = layer.HasTiming,
                avgCached = CacheHealth.FormatMilliseconds(layer.AvgCachedMs) ?? cannot,
                avgUncached = CacheHealth.FormatMilliseconds(layer.AvgUncachedMs) ?? cannot,
                improvement = CacheHealth.FormatImprovement(layer.ImprovementPercent) ?? cannot,
                saved = CacheHealth.FormatSaved(layer.SavedMs) ?? cannot,
                dbAvoided = layer.EstimatedDatabaseOperationsAvoided,
                cachedBarPercent = cachedBar
            };
        }

        private static object ToEntryPayload(CacheEntrySnapshot entry)
        {
            return new
            {
                key = entry.Key,
                displayName = entry.DisplayName,
                cacheKind = entry.CacheKind,
                category = entry.Category,
                status = entry.Status,
                typeName = entry.TypeName,
                size = entry.Size,
                created = FormatTimestamp(entry.CreatedUtc),
                expires = FormatTimestamp(entry.ExpiresUtc),
                ttl = entry.Ttl,
                hitCount = entry.HitCount,
                misses = entry.Misses,
                missCount = entry.MissCount,
                hitRatio = entry.HitRatioPercent,
                avgCachedMs = entry.AvgCachedMs,
                avgUncachedMs = entry.AvgUncachedMs,
                improvementPercent = entry.ImprovementPercent,
                lastAccess = FormatTimestamp(entry.LastAccessUtc),
                slidingSeconds = entry.SlidingSeconds
            };
        }

        private static string FormatTimestamp(DateTimeOffset? value)
        {
            return value.HasValue
                ? value.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : CacheDiagnostics.NotAvailable;
        }

        private static object ToExportRow(CacheEntrySnapshot entry)
        {
            entry = entry ?? new CacheEntrySnapshot();
            long missCount;
            if (!long.TryParse(entry.MissCount, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out missCount))
            {
                missCount = 0;
            }

            var reads = entry.HitCount + missCount;
            var ratio = reads <= 0
                ? CacheDiagnostics.NotAvailable
                : Math.Round(entry.HitCount / (double)reads * 100d, 2, MidpointRounding.AwayFromZero)
                    .ToString("0.00", CultureInfo.InvariantCulture);

            return new
            {
                CacheName = entry.DisplayName ?? "",
                CacheKey = entry.Key ?? "",
                CacheKind = entry.CacheKind ?? "",
                Category = entry.Category ?? "",
                Status = entry.Status ?? "",
                Health = ResolveHealth(entry),
                HitCount = entry.HitCount,
                MissCount = missCount,
                EntryHitRatioPercent = ratio,
                AvgCachedMs = FormatOptionalMs(entry.AvgCachedMs),
                AvgUncachedMs = FormatOptionalMs(entry.AvgUncachedMs),
                ImprovementPercent = entry.ImprovementPercent.HasValue
                    ? entry.ImprovementPercent.Value.ToString("0.0", CultureInfo.InvariantCulture)
                    : CacheDiagnostics.NotAvailable,
                Ttl = entry.Ttl ?? CacheDiagnostics.NotAvailable,
                TypeName = entry.TypeName ?? CacheDiagnostics.NotAvailable,
                Created = FormatTimestamp(entry.CreatedUtc),
                Expires = FormatTimestamp(entry.ExpiresUtc),
                LastAccess = FormatTimestamp(entry.LastAccessUtc),
                SlidingSeconds = entry.SlidingSeconds ?? CacheDiagnostics.NotAvailable,
                Size = entry.Size ?? CacheDiagnostics.NotAvailable
            };
        }

        private static string FormatOptionalMs(double? ms)
        {
            return ms.HasValue
                ? ms.Value.ToString("0.00", CultureInfo.InvariantCulture)
                : CacheDiagnostics.NotAvailable;
        }

        private static string ResolveHealth(CacheEntrySnapshot entry)
        {
            if (entry == null)
            {
                return CacheDiagnostics.NotAvailable;
            }

            if (string.Equals(entry.Status, CacheDiagnostics.StatusExpired, StringComparison.OrdinalIgnoreCase))
            {
                return "Expired";
            }

            return entry.HitCount > 0 ? "Healthy" : "Cold";
        }

        private static System.Data.DataTable NamedTable(System.Data.DataTable table, string name)
        {
            if (table != null)
            {
                table.TableName = name;
            }

            return table;
        }

        private static System.Data.DataTable EnsureColumns(System.Data.DataTable table, Func<System.Data.DataTable> columnsFactory)
        {
            if (table != null && table.Columns.Count > 0)
            {
                return table;
            }

            var withColumns = columnsFactory();
            withColumns.Rows.Clear();
            return withColumns;
        }

        private string NormalizeClearCacheReturnUrl(string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return Url.Action("Index", "Cache", new { area = AdminAreaName });
            }

            try
            {
                var uri = new Uri(redirectUrl, UriKind.RelativeOrAbsolute);
                var path = uri.IsAbsoluteUri ? uri.AbsolutePath : redirectUrl.Split('?')[0];
                if (path.IndexOf("uploadwebsitelogo", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var id = 0;
                    if (uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Query))
                    {
                        var query = HttpUtility.ParseQueryString(uri.Query);
                        int.TryParse(query["id"], out id);
                    }

                    if (id > 0)
                    {
                        return Url.Action("WebSiteLogo", "Settings", new { area = AdminAreaName, id });
                    }

                    return Url.Action("AddWebSiteLogo", "Settings", new { area = AdminAreaName });
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "NormalizeClearCacheReturnUrl failed for {0}", redirectUrl);
                return Url.Action("Index", "Cache", new { area = AdminAreaName });
            }

            return redirectUrl;
        }
    }
}
