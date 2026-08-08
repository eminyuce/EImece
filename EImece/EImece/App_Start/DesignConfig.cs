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
                        var cssBundle = new StyleBundle($"~/Content/designs/{designName}/css");
                        cssBundle.IncludeDirectory($"~/Content/designs/{designName}/css", "*.css", true);
                        bundles.Add(cssBundle);
                    }

                    string jsDir = Path.Combine(dir, "js");
                    if (Directory.Exists(jsDir))
                    {
                        var jsBundle = new ScriptBundle($"~/bundles/designs/{designName}/js");
                        jsBundle.IncludeDirectory($"~/Content/designs/{designName}/js", "*.js", true);
                        bundles.Add(jsBundle);
                    }
                }
            }
        }
    }
}
