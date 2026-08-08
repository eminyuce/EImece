using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EImece.Infrastructure.Designs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Infrastructure
{
    [TestClass]
    public class DesignSystemTests
    {
        private class TestDesignProvider : IDesignProvider
        {
            public string ActiveDesign { get; set; } = "Modern";
            public string GetActiveDesign() => ActiveDesign;
        }

        private class DummyBrowserCapabilities : HttpBrowserCapabilitiesBase
        {
            public override bool IsMobileDevice => false;
            public override string this[string key] => "false";
        }

        private class DummyHttpRequest : HttpRequestBase
        {
            public override string UserAgent => "Mozilla/5.0";
            public override System.Collections.Specialized.NameValueCollection Headers => new System.Collections.Specialized.NameValueCollection();
            public override HttpCookieCollection Cookies => new HttpCookieCollection();
            public override HttpBrowserCapabilitiesBase Browser { get; } = new DummyBrowserCapabilities();
        }

        private class DummyHttpResponse : HttpResponseBase
        {
            public override HttpCookieCollection Cookies => new HttpCookieCollection();
        }

        private class DummyHttpContext : HttpContextBase
        {
            public override System.Collections.IDictionary Items { get; } = new System.Collections.Hashtable();
            public override HttpRequestBase Request { get; } = new DummyHttpRequest();
            public override HttpResponseBase Response { get; } = new DummyHttpResponse();
        }

        private class DummyController : ControllerBase
        {
            protected override void ExecuteCore()
            {
            }
        }

        private ControllerContext CreateControllerContext(string controllerName = "Products", string actionName = "Detail", string areaName = null)
        {
            var routeData = new RouteData();
            routeData.Values["controller"] = controllerName;
            routeData.Values["action"] = actionName;
            if (!string.IsNullOrEmpty(areaName))
            {
                routeData.DataTokens["area"] = areaName;
            }

            return new ControllerContext(new DummyHttpContext(), routeData, new DummyController());
        }

        [TestMethod]
        public void ExistingDesignView_RendersSuccessfully()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            var engine = new DesignAwareRazorViewEngine(designProvider);
            engine.FileExistsOverride = (path) => path.Equals("~/Views/Designs/Modern/Products/Detail.cshtml", StringComparison.OrdinalIgnoreCase);

            var context = CreateControllerContext("Products", "Detail");
            var result = engine.FindView(context, "Detail", null, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.View);
        }

        [TestMethod]
        public void MissingDesignView_ThrowsMissingDesignViewException()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            var engine = new DesignAwareRazorViewEngine(designProvider);
            engine.FileExistsOverride = (path) => false; // File does not exist in design

            var context = CreateControllerContext("Products", "NonExistentAction");

            try
            {
                engine.FindView(context, "NonExistentAction", null, false);
                Assert.Fail("Expected MissingDesignViewException was not thrown.");
            }
            catch (MissingDesignViewException ex)
            {
                Assert.AreEqual("Modern", ex.Design);
                Assert.AreEqual("Products", ex.Controller);
                Assert.AreEqual("NonExistentAction", ex.Action);
            }
        }

        [TestMethod]
        public void DefaultViewExists_DesignViewMissing_MustStillFailNoFallback()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            var engine = new DesignAwareRazorViewEngine(designProvider);

            // Simulate default view exists (~/Views/Products/Detail.cshtml) but design view (~/Views/Designs/Modern/Products/Detail.cshtml) does NOT exist
            engine.FileExistsOverride = (path) => path.Equals("~/Views/Products/Detail.cshtml", StringComparison.OrdinalIgnoreCase);

            var context = CreateControllerContext("Products", "Detail");

            try
            {
                engine.FindView(context, "Detail", null, false);
                Assert.Fail("Expected MissingDesignViewException when design view is missing.");
            }
            catch (MissingDesignViewException ex)
            {
                Assert.AreEqual("Modern", ex.Design);
                Assert.AreEqual("Products", ex.Controller);
                Assert.AreEqual("Detail", ex.ViewName);
            }
        }

        [TestMethod]
        public void SharedViewMissing_MustStillFailNoFallback()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            var engine = new DesignAwareRazorViewEngine(designProvider);

            // Simulate root default shared footer exists, but design shared footer is missing
            engine.FileExistsOverride = (path) => path.Equals("~/Views/Shared/_Footer.cshtml", StringComparison.OrdinalIgnoreCase);

            var context = CreateControllerContext("Products", "Detail");

            try
            {
                engine.FindPartialView(context, "_Footer", false);
                Assert.Fail("Expected MissingDesignViewException when design shared footer is missing.");
            }
            catch (MissingDesignViewException ex)
            {
                Assert.AreEqual("Modern", ex.Design);
                Assert.AreEqual("_Footer", ex.ViewName);
            }
        }

        [TestMethod]
        public void AlternateDesign_ResolvesAlternateDesignView()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Minimal" };
            var engine = new DesignAwareRazorViewEngine(designProvider);

            engine.FileExistsOverride = (path) => path.Equals("~/Views/Designs/Minimal/Products/Detail.cshtml", StringComparison.OrdinalIgnoreCase);

            var context = CreateControllerContext("Products", "Detail");
            var result = engine.FindView(context, "Detail", null, false);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.View);
        }

        [TestMethod]
        public void AdminArea_UnaffectedByActiveDesign()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            var engine = new DesignAwareRazorViewEngine(designProvider);
            bool designPathProbed = false;
            engine.FileExistsOverride = (path) =>
            {
                if (path.StartsWith("~/Views/Designs/", StringComparison.OrdinalIgnoreCase))
                {
                    designPathProbed = true;
                }
                return true;
            };

            var context = CreateControllerContext("Dashboard", "Index", areaName: "Admin");
            var result = engine.FindView(context, "Index", null, false);

            Assert.IsNotNull(result);
            Assert.IsFalse(designPathProbed, "Admin area should not probe design paths under ~/Views/Designs/");
        }

        [TestMethod]
        public void LayoutResolver_ThrowsWhenDesignLayoutMissing()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            DesignPathResolver.SetDesignProvider(designProvider);
            DesignPathResolver.FileExistsOverride = (path) => false;

            try
            {
                DesignPathResolver.ResolveLayout("_Layout");
                Assert.Fail("Expected MissingDesignViewException when layout is missing.");
            }
            catch (MissingDesignViewException ex)
            {
                Assert.AreEqual("Modern", ex.Design);
            }
        }

        [TestMethod]
        public void LayoutResolver_ReturnsDesignLayoutWhenExists()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            DesignPathResolver.SetDesignProvider(designProvider);
            DesignPathResolver.FileExistsOverride = (path) => path.Equals("~/Views/Designs/Modern/Shared/_Layout.cshtml", StringComparison.OrdinalIgnoreCase);

            string layoutPath = DesignPathResolver.ResolveLayout("_Layout");
            Assert.AreEqual("~/Views/Designs/Modern/Shared/_Layout.cshtml", layoutPath);
        }

        [TestMethod]
        public void DesignAssetHelper_ResolvesDesignAssetPath()
        {
            var designProvider = new TestDesignProvider { ActiveDesign = "Modern" };
            DesignHtmlHelpers.SetDesignProvider(designProvider);

            HtmlHelper html = null;
            string assetUrl = html.DesignAsset("css/theme.css");

            Assert.AreEqual("~/Content/designs/modern/css/theme.css", assetUrl);
        }

        private string GetAppRoot()
        {
            string asmPath = typeof(DesignAwareRazorViewEngine).Assembly.Location;
            string dir = Path.GetDirectoryName(asmPath);
            while (!string.IsNullOrEmpty(dir))
            {
                string projPath = Path.Combine(dir, "EImece.csproj");
                if (File.Exists(projPath))
                {
                    return dir;
                }
                string subProjPath = Path.Combine(dir, "EImece", "EImece.csproj");
                if (File.Exists(subProjPath))
                {
                    return Path.Combine(dir, "EImece");
                }
                dir = Directory.GetParent(dir)?.FullName;
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        [TestMethod]
        public void DesignValidator_ValidatesDesignCompleteness()
        {
            string appRoot = GetAppRoot();
            var result = DesignValidator.ValidateDesign("Modern", appRoot);

            if (!result.IsValid)
            {
                Assert.Fail($"AppRoot: {appRoot}, Total: {result.TotalRequiredViews}, Missing ({result.MissingViews.Count}): {string.Join(", ", result.MissingViews)}");
            }

            Assert.IsTrue(result.TotalRequiredViews > 0);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void DesignValidator_ValidatesModernBootstrapCompleteness()
        {
            string appRoot = GetAppRoot();
            var result = DesignValidator.ValidateDesign("Modern-Bootstrap", appRoot);

            if (!result.IsValid)
            {
                Assert.Fail($"AppRoot: {appRoot}, Total: {result.TotalRequiredViews}, Missing ({result.MissingViews.Count}): {string.Join(", ", result.MissingViews)}");
            }

            Assert.IsTrue(result.TotalRequiredViews > 0);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void DesignValidator_DetectsMissingViewsInIncompleteDesign()
        {
            string appRoot = GetAppRoot();
            var result = DesignValidator.ValidateDesign("IncompleteDesignTest", appRoot);

            if (result.IsValid)
            {
                Assert.Fail($"AppRoot: {appRoot}, Expected IncompleteDesignTest to be invalid, but Total: {result.TotalRequiredViews}, Missing: {result.MissingViews.Count}");
            }

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MissingViews.Count > 0);
        }
    }
}
