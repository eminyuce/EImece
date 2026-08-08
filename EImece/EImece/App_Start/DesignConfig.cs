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

            // Plain Bundle (not StyleBundle) — WebGrease CssMinify misparses CSS custom properties (--crizal-*).
            bundles.Add(new Bundle("~/bundles/designs/crizal/vendor/css").Include(
                "~/Content/designs/crizal/vendor/css/styles.css",
                "~/Content/designs/crizal/search/search.css",
                "~/Content/designs/crizal/css/theme.css",
                "~/Content/designs/crizal/css/components.css",
                "~/Content/designs/crizal/css/responsive.css"));

            bundles.Add(new Bundle("~/bundles/designs/crizal/vendor/js").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Content/designs/crizal/vendor/js/popper.min.js",
                "~/Content/designs/crizal/vendor/js/bootstrap.min.js",
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
