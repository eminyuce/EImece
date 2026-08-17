using System;
using System.IO;
using System.Web.Hosting;
using System.Web.Optimization;

namespace EImece.App_Start
{
    public static class DesignConfig
    {
        public static void RegisterDesignBundles(BundleCollection bundles)
        {
            if (bundles == null) return;

            // Bundle path lives under plugins/ so relative font url(../../fonts/...) resolve correctly.
            // StyleBundle enables CssMinify (no CSS custom properties in these vendor files).
            bundles.Add(new StyleBundle("~/Content/designs/crizal/vendor/css/plugins/crizal-plugins").Include(
                "~/Content/designs/crizal/vendor/css/plugins/bootstrap.min.css",
                "~/Content/designs/crizal/vendor/css/plugins/animate.css",
                "~/Content/designs/crizal/vendor/css/plugins/animated-headline.css",
                "~/Content/designs/crizal/vendor/css/plugins/fontawesome-all.min.css",
                "~/Content/designs/crizal/vendor/css/plugins/et-line.css",
                "~/Content/designs/crizal/vendor/css/plugins/themify-icons.css",
                "~/Content/designs/crizal/vendor/css/plugins/magnific-popup.css",
                "~/Content/designs/crizal/vendor/css/plugins/owl.carousel.css",
                "~/Content/designs/crizal/vendor/css/plugins/owl.theme.default.css",
                "~/Content/designs/crizal/vendor/css/plugins/odometer-theme-default.css",
                "~/Content/designs/crizal/vendor/css/plugins/lightgallery.css",
                "~/Content/designs/crizal/vendor/css/plugins/xzoom.css",
                "~/Content/designs/crizal/vendor/css/plugins/default.css",
                "~/Content/designs/crizal/vendor/css/plugins/nav-menu.css",
                "~/Content/designs/crizal/vendor/css/plugins/prism.css"));

            // Plain Bundle (not StyleBundle) — WebGrease CssMinify misparses CSS custom properties (--crizal-*).
            bundles.Add(new Bundle("~/bundles/designs/crizal/vendor/css").Include(
                "~/Content/designs/crizal/vendor/css/styles.css",
                "~/Content/designs/crizal/search/search.css",
                "~/Content/designs/crizal/css/theme.css",
                "~/Content/designs/crizal/css/components.css",
                "~/Content/designs/crizal/css/responsive.css",
                "~/Content/pageThemes.css"));

            // Plain Bundle — WebGrease JsMinify fails on modern syntax in main.js / theme.js.
            bundles.Add(new Bundle("~/bundles/designs/crizal/vendor/js").Include(
                "~/Scripts/admin-vendor/jquery-4.0.0.js",
                "~/Scripts/admin-vendor/jquery-migrate-4.0.2.js",
                "~/Scripts/admin-vendor/bootstrap.bundle.min.js",
                "~/Content/designs/crizal/vendor/js/jquery.magnific-popup.min.js",
                "~/Content/designs/crizal/vendor/js/jarallax.min.js",
                "~/Content/designs/crizal/vendor/js/nav-menu.js",
                "~/Content/designs/crizal/vendor/js/owl.carousel.js",
                "~/Content/designs/crizal/vendor/js/wow.js",
                "~/Content/designs/crizal/vendor/js/odometer.min.js",
                "~/Content/designs/crizal/vendor/js/main.js",
                "~/Content/designs/crizal/js/theme.js"));

            string contentPath = HostingEnvironment.IsHosted
                ? HostingEnvironment.MapPath("~/Content/designs")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "designs");

            if (!string.IsNullOrEmpty(contentPath) && Directory.Exists(contentPath))
            {
                var designDirs = Directory.GetDirectories(contentPath);
                foreach (var dir in designDirs)
                {
                    string designName = Path.GetFileName(dir).ToLowerInvariant();

                    string cssDir = Path.Combine(dir, "css");
                    if (Directory.Exists(cssDir))
                    {
                        var cssBundle = new Bundle($"~/bundles/designs/{designName}/css");
                        cssBundle.IncludeDirectory($"~/Content/designs/{designName}/css", "*.css", true);
                        bundles.Add(cssBundle);
                    }

                    string jsDir = Path.Combine(dir, "js");
                    if (Directory.Exists(jsDir))
                    {
                        var jsBundle = new Bundle($"~/bundles/designs/{designName}/js");
                        jsBundle.IncludeDirectory($"~/Content/designs/{designName}/js", "*.js", true);
                        bundles.Add(jsBundle);
                    }
                }
            }
        }
    }
}
