using System;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;

namespace EImece.Infrastructure.Designs
{
    public static class DesignPathResolver
    {
        private static IDesignProvider _designProvider = new ConfigDesignProvider();

        public static Func<string, bool> FileExistsOverride { get; set; }

        public static void SetDesignProvider(IDesignProvider provider)
        {
            _designProvider = provider ?? new ConfigDesignProvider();
        }

        public static string ResolveLayout(string layoutName, string area = null, ViewContext viewContext = null)
        {
            if (string.IsNullOrEmpty(layoutName))
            {
                return null;
            }

            // If layoutName is already an explicit virtual path to a design layout, return it
            if (layoutName.StartsWith("~/Views/Designs/", StringComparison.OrdinalIgnoreCase))
            {
                return layoutName;
            }

            string activeDesign = _designProvider.GetActiveDesign();

            // Admin area layout stays unchanged
            if (string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return "~/Areas/Admin/Views/Shared/_Layout.cshtml";
            }

            if (string.IsNullOrEmpty(activeDesign))
            {
                if (string.Equals(area, "Customers", StringComparison.OrdinalIgnoreCase))
                {
                    return "~/Areas/Customers/Views/Shared/_Layout.cshtml";
                }
                return "~/Views/Shared/_Layout.cshtml";
            }

            string layoutFileName = layoutName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                ? layoutName
                : layoutName + ".cshtml";

            string expectedPath;
            if (!string.IsNullOrEmpty(area))
            {
                expectedPath = $"~/Views/Designs/{activeDesign}/Areas/{area}/Shared/{layoutFileName}";
                if (DoesFileExist(expectedPath))
                {
                    return expectedPath;
                }
            }

            expectedPath = $"~/Views/Designs/{activeDesign}/Shared/{layoutFileName}";
            if (DoesFileExist(expectedPath))
            {
                return expectedPath;
            }

            string controllerName = viewContext?.RouteData?.GetRequiredString("controller") ?? "Unknown";
            string actionName = viewContext?.RouteData?.Values["action"]?.ToString() ?? "Unknown";

            throw new MissingDesignViewException(
                activeDesign,
                controllerName,
                actionName,
                layoutName,
                expectedPath,
                area);
        }

        private static bool DoesFileExist(string virtualPath)
        {
            if (FileExistsOverride != null)
            {
                return FileExistsOverride(virtualPath);
            }

            if (HostingEnvironment.IsHosted && HostingEnvironment.VirtualPathProvider != null)
            {
                return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
            }

            string appDomainPath = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = virtualPath.Replace("~/", "").Replace('/', Path.DirectorySeparatorChar);
            string physicalPath = Path.Combine(appDomainPath, relativePath);
            return File.Exists(physicalPath);
        }
    }
}
