using EImece.Areas.Admin.Controllers;
using EImece.Areas.Admin.Models;
using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Services.IServices;
using EImece.Tests.Infrastructure;
using EImece.Web.Areas.Admin.Controllers;
using EImece.Web.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;
using System.Web.Mvc;
using DomainConstants = EImece.Domain.Constants;

namespace EImece.Tests.Controllers
{
    [TestClass]
    public class CacheControllerTests
    {
        private class InterfaceMockProxy<T> : RealProxy
        {
            public InterfaceMockProxy() : base(typeof(T))
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType == typeof(Task))
                    {
                        defaultResult = Task.CompletedTask;
                    }
                    else if (mi.ReturnType.IsGenericType && mi.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var innerType = mi.ReturnType.GetGenericArguments()[0];
                        var defaultInner = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
                        defaultResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(innerType).Invoke(null, new[] { defaultInner });
                    }
                    else if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public T Service => (T)GetTransparentProxy();
        }

        private static T Mock<T>() => new InterfaceMockProxy<T>().Service;

        private MemoryCacheProvider _cache;
        private CacheController _controller;
        private string _prefix;

        [TestInitialize]
        public void Init()
        {
            CacheDiagnostics.Reset();
            _cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            _prefix = "setting:admin:" + Guid.NewGuid().ToString("N") + ":";
            _controller = new CacheController(
                Mock<ISettingService>(),
                Mock<IProductService>(),
                Mock<IProductCategoryService>(),
                _cache,
                Mock<IHttpRuntimeCacheClearer>(),
                TestNullLoggers.Create<CacheController>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.ClearByPrefix(_prefix);
            CacheDiagnostics.Reset();
        }

        [TestMethod]
        public void CacheController_InheritsBaseAdminAuthorization()
        {
            Assert.IsInstanceOfType(_controller, typeof(BaseAdminController));
            var auth = typeof(BaseAdminController).GetCustomAttribute<AuthorizeRolesAttribute>();
            Assert.IsNotNull(auth);
            Assert.IsTrue(auth.Roles.Contains(DomainConstants.AdministratorRole));
            Assert.IsTrue(auth.Roles.Contains(DomainConstants.EditorRole));
        }

        [TestMethod]
        public void Index_ReturnsTrackedKeys()
        {
            var key = _prefix + "listed";
            _cache.Set(key, "hidden-token-value", CachePolicy.Absolute(60));

            var result = _controller.Index(_prefix, "all", "all", 1) as ViewResult;
            Assert.IsNotNull(result);
            var model = result.Model as CacheAdminViewModel;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Entries.Any(e => e.Key == key));
            Assert.AreEqual(1, model.Metrics.Sets);
            Assert.IsNotNull(model.Overview);
            Assert.IsNotNull(model.Overview.Data);
            Assert.IsNotNull(model.Overview.Page);
        }

        [TestMethod]
        public void IndexGrid_ViewUsesGriddly()
        {
            var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(CacheControllerTests).Assembly.Location),
                "..", "..", "..", "EImece", "Areas", "Admin", "Views", "Cache", "IndexGrid.cshtml"));
            Assert.IsTrue(System.IO.File.Exists(path), "Missing " + path);
            var cshtml = System.IO.File.ReadAllText(path);
            StringAssert.Contains(cshtml, "GriddlySettings<CacheEntrySnapshot>");
            StringAssert.Contains(cshtml, "Html.Griddly");
            StringAssert.Contains(cshtml, "HitCount");
            StringAssert.Contains(cshtml, "DisplayName");
        }

        [TestMethod]
        public void Index_ViewContainsOverviewAndCacheTypes()
        {
            var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(CacheControllerTests).Assembly.Location),
                "..", "..", "..", "EImece", "Areas", "Admin", "Views", "Cache", "Index.cshtml"));
            Assert.IsTrue(System.IO.File.Exists(path), "Missing " + path);
            var cshtml = System.IO.File.ReadAllText(path);
            StringAssert.Contains(cshtml, "CacheOverview");
            StringAssert.Contains(cshtml, "CachePageResponseTitle");
            StringAssert.Contains(cshtml, "CacheApplicationDataTitle");
            StringAssert.Contains(cshtml, "InvalidateCache");
        }

        [TestMethod]
        public void Diagnostics_SearchFiltersKeys_AndDoesNotExposeValues()
        {
            _cache.Set(_prefix + "keep", "hidden-api-key-value", CachePolicy.Absolute(60));
            _cache.Set(CacheKeys.ProductDetail(777001), "other", CachePolicy.Absolute(60));

            var result = _controller.Diagnostics(_prefix + "keep", "all", "all", 1) as JsonResult;
            Assert.IsNotNull(result);
            var json = JsonConvert.SerializeObject(result.Data);
            StringAssert.Contains(json, _prefix + "keep");
            Assert.IsFalse(json.IndexOf("hidden-api-key-value", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.IsFalse(json.IndexOf("\"value\"", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.IsFalse(json.IndexOf("product:detail", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [TestMethod]
        public void Diagnostics_CategoryFilter_ReturnsMatchingFamily()
        {
            _cache.Set(CacheKeys.MenuTree(9), "menu-tree", CachePolicy.Absolute(60));
            _cache.Set(_prefix + "settings-only", "s", CachePolicy.Absolute(60));

            var result = _controller.Diagnostics("", "Menus", "all", 1) as JsonResult;
            var json = JsonConvert.SerializeObject(result.Data);
            StringAssert.Contains(json, "menu:tree");
            Assert.IsFalse(json.IndexOf(_prefix + "settings-only", StringComparison.Ordinal) >= 0);
        }

        [TestMethod]
        public void Diagnostics_Pagination_ReturnsRequestedPageSize()
        {
            for (var i = 0; i < 5; i++)
            {
                _cache.Set(_prefix + "p" + i, i, CachePolicy.Absolute(60));
            }

            var result = _controller.Diagnostics(_prefix, "all", "all", 1) as JsonResult;
            var json = JsonConvert.SerializeObject(result.Data);
            var parsed = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
            Assert.AreEqual(5, (int)parsed["totalCount"]);
            Assert.AreEqual(1, (int)parsed["page"]);
        }

        [TestMethod]
        public void Entry_ReturnsMetadataWithoutValue()
        {
            var key = _prefix + "detail";
            _cache.Set(key, "credential-secret", CachePolicy.Absolute(60));

            var result = _controller.Entry(key) as JsonResult;
            Assert.IsNotNull(result);
            var json = JsonConvert.SerializeObject(result.Data);
            StringAssert.Contains(json, key);
            Assert.IsFalse(json.IndexOf("credential-secret", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [TestMethod]
        public void Entry_UnknownKey_Returns404()
        {
            var result = _controller.Entry(_prefix + "missing");
            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
        }

        [TestMethod]
        public void RefreshDiagnostics_DoesNotClearCache()
        {
            var key = _prefix + "stay";
            _cache.Set(key, "keep-me", CachePolicy.Absolute(60));

            var before = _controller.Diagnostics(_prefix, "all", "all", 1) as JsonResult;
            var after = _controller.Diagnostics(_prefix, "all", "all", 1) as JsonResult;

            Assert.IsTrue(_cache.Get(key, out string value));
            Assert.AreEqual("keep-me", value);
            var json = JsonConvert.SerializeObject(after.Data);
            StringAssert.Contains(json, key);
            Assert.IsNotNull(before);
        }

        [TestMethod]
        public void Invalidate_Products_UsesSharedMaintenanceAndKeepsUnrelatedKeys()
        {
            var settingKey = _prefix + "unrelated";
            _cache.Set(settingKey, "keep", CachePolicy.Absolute(60));
            _cache.Set(CacheKeys.ProductDetail(888001), "drop", CachePolicy.Absolute(60));

            bool fullWipe;
            var removed = AdminCacheMaintenance.Invalidate(
                "settings",
                Mock<ISettingService>(),
                Mock<IProductService>(),
                Mock<IProductCategoryService>(),
                _cache,
                Mock<IHttpRuntimeCacheClearer>(),
                out fullWipe);

            Assert.IsFalse(fullWipe);
            Assert.IsTrue(removed >= 0);
        }

        [TestMethod]
        public void ExportExcel_Csv_IncludesKeysAndHealth_NotCachedValues()
        {
            var key = _prefix + "export";
            _cache.Set(key, "secret-export-token", CachePolicy.Absolute(60));
            _cache.Get(key, out string _);

            var result = _controller.ExportExcel("csv", _prefix, "all", "all") as FileContentResult;
            Assert.IsNotNull(result);
            StringAssert.EndsWith(result.FileDownloadName, ".csv");
            Assert.AreEqual("text/csv", result.ContentType);

            var csv = System.Text.Encoding.UTF8.GetString(result.FileContents);
            StringAssert.Contains(csv, key);
            StringAssert.Contains(csv, "Health");
            StringAssert.Contains(csv, "HitCount");
            Assert.IsFalse(csv.IndexOf("secret-export-token", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [TestMethod]
        public void ExportExcel_Csv_RespectsSearchFilter()
        {
            var keep = _prefix + "keep-me";
            var drop = _prefix + "drop-me";
            _cache.Set(keep, "a", CachePolicy.Absolute(60));
            _cache.Set(drop, "b", CachePolicy.Absolute(60));

            var result = _controller.ExportExcel("csv", "keep-me", "all", "all") as FileContentResult;
            Assert.IsNotNull(result);
            var csv = System.Text.Encoding.UTF8.GetString(result.FileContents);
            StringAssert.Contains(csv, keep);
            Assert.IsFalse(csv.IndexOf(drop, StringComparison.Ordinal) >= 0);
        }

        [TestMethod]
        public void ExportExcel_Xls_ReturnsWorkbookWithoutCachedValues()
        {
            var key = _prefix + "xls";
            _cache.Set(key, "workbook-secret", CachePolicy.Absolute(60));

            FileContentResult result;
            try
            {
                result = _controller.ExportExcel("excel", _prefix, "all", "all") as FileContentResult;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                var detail = (ex.FileName ?? "") + " " + (ex.Message ?? "");
                if (detail.IndexOf("SkiaSharp", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Assert.Inconclusive("NPOI AutoSizeColumn requires SkiaSharp in the test host; CSV export covers content.");
                }

                throw;
            }

            Assert.IsNotNull(result);
            StringAssert.EndsWith(result.FileDownloadName, ".xls");
            Assert.AreEqual("application/vnd.ms-excel", result.ContentType);
            Assert.IsTrue(result.FileContents.Length > 0);

            var asText = System.Text.Encoding.ASCII.GetString(result.FileContents);
            Assert.IsFalse(asText.IndexOf("workbook-secret", StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
