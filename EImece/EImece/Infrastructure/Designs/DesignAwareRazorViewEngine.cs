using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace EImece.Infrastructure.Designs
{
    public class DesignAwareRazorViewEngine : RazorViewEngine
    {
        private readonly IDesignProvider _designProvider;
        public Func<string, bool> FileExistsOverride { get; set; }

        public DesignAwareRazorViewEngine() : this(new ConfigDesignProvider())
        {
        }

        public DesignAwareRazorViewEngine(IDesignProvider designProvider)
        {
            _designProvider = designProvider ?? new ConfigDesignProvider();
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(viewName));
            }

            string activeDesign = _designProvider.GetActiveDesign();
            string areaName = GetAreaName(controllerContext);
            string controllerName = controllerContext.RouteData?.GetRequiredString("controller") ?? string.Empty;
            string actionName = controllerContext.RouteData?.Values["action"]?.ToString() ?? viewName;

            // Admin area and standalone admin entry views are excluded from design overrides
            if (IsAdminArea(areaName) || IsDesignExcludedView(controllerName, viewName) || string.IsNullOrEmpty(activeDesign))
            {
                if (FileExistsOverride != null)
                {
                    string path = string.IsNullOrEmpty(areaName)
                        ? $"~/Views/{controllerName}/{viewName}.cshtml"
                        : $"~/Areas/{areaName}/Views/{controllerName}/{viewName}.cshtml";

                    if (FileExistsOverride(path))
                    {
                        IView v = CreateView(controllerContext, path, masterName);
                        return new ViewEngineResult(v, this);
                    }
                    return new ViewEngineResult(new[] { path });
                }

                return base.FindView(controllerContext, viewName, masterName, useCache);
            }

            List<string> searchedLocations = new List<string>();
            string resolvedPath = ResolveDesignViewPath(activeDesign, areaName, controllerName, viewName, searchedLocations);

            if (!string.IsNullOrEmpty(resolvedPath))
            {
                IView view = CreateView(controllerContext, resolvedPath, masterName);
                return new ViewEngineResult(view, this);
            }

            string expectedPath = searchedLocations.Count > 0 ? searchedLocations[0] : $"~/Views/Designs/{activeDesign}/{controllerName}/{viewName}.cshtml";
            throw new MissingDesignViewException(
                activeDesign,
                controllerName,
                actionName,
                viewName,
                expectedPath,
                areaName,
                searchedLocations.ToArray());
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }

            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(partialViewName));
            }

            string activeDesign = _designProvider.GetActiveDesign();
            string areaName = GetAreaName(controllerContext);
            string controllerName = controllerContext.RouteData?.GetRequiredString("controller") ?? string.Empty;
            string actionName = controllerContext.RouteData?.Values["action"]?.ToString() ?? partialViewName;

            // Admin area is excluded from design overrides
            if (IsAdminArea(areaName) || string.IsNullOrEmpty(activeDesign))
            {
                if (FileExistsOverride != null)
                {
                    string path = string.IsNullOrEmpty(areaName)
                        ? $"~/Views/{controllerName}/{partialViewName}.cshtml"
                        : $"~/Areas/{areaName}/Views/{controllerName}/{partialViewName}.cshtml";

                    if (FileExistsOverride(path))
                    {
                        IView v = CreatePartialView(controllerContext, path);
                        return new ViewEngineResult(v, this);
                    }
                    return new ViewEngineResult(new[] { path });
                }

                return base.FindPartialView(controllerContext, partialViewName, useCache);
            }

            List<string> searchedLocations = new List<string>();
            string resolvedPath = ResolveDesignViewPath(activeDesign, areaName, controllerName, partialViewName, searchedLocations);

            if (!string.IsNullOrEmpty(resolvedPath))
            {
                IView view = CreatePartialView(controllerContext, resolvedPath);
                return new ViewEngineResult(view, this);
            }

            bool isBuiltInTemplate = partialViewName.StartsWith("EditorTemplates/", StringComparison.OrdinalIgnoreCase) ||
                                     partialViewName.StartsWith("DisplayTemplates/", StringComparison.OrdinalIgnoreCase);

            if (isBuiltInTemplate)
            {
                return base.FindPartialView(controllerContext, partialViewName, useCache);
            }

            string expectedPath = searchedLocations.Count > 0 ? searchedLocations[0] : $"~/Views/Designs/{activeDesign}/{controllerName}/{partialViewName}.cshtml";
            throw new MissingDesignViewException(
                activeDesign,
                controllerName,
                actionName,
                partialViewName,
                expectedPath,
                areaName,
                searchedLocations.ToArray());
        }

        private string ResolveDesignViewPath(string design, string area, string controller, string viewName, List<string> searchedLocations)
        {
            // Format view name extension if missing
            string viewNameWithExt = viewName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ? viewName : viewName + ".cshtml";

            // Case A: Specific path starting with ~ or /
            if (viewName.StartsWith("~/", StringComparison.Ordinal) || viewName.StartsWith("/", StringComparison.Ordinal))
            {
                string mappedPath;
                if (viewName.StartsWith("~/Views/Designs/", StringComparison.OrdinalIgnoreCase))
                {
                    mappedPath = viewName;
                }
                else if (viewName.StartsWith("~/Areas/", StringComparison.OrdinalIgnoreCase))
                {
                    // ~/Areas/Customers/Views/Home/Index.cshtml -> ~/Views/Designs/{design}/Areas/Customers/Home/Index.cshtml
                    string pathWithoutAreas = viewName.Substring("~/Areas/".Length);
                    int firstSlash = pathWithoutAreas.IndexOf('/');
                    if (firstSlash > 0)
                    {
                        string targetArea = pathWithoutAreas.Substring(0, firstSlash);
                        string rest = pathWithoutAreas.Substring(firstSlash).Replace("/Views/", "/");
                        mappedPath = $"~/Views/Designs/{design}/Areas/{targetArea}{rest}";
                    }
                    else
                    {
                        mappedPath = $"~/Views/Designs/{design}/{pathWithoutAreas}";
                    }
                }
                else if (viewName.StartsWith("~/Views/", StringComparison.OrdinalIgnoreCase))
                {
                    mappedPath = $"~/Views/Designs/{design}/" + viewName.Substring("~/Views/".Length);
                }
                else
                {
                    mappedPath = $"~/Views/Designs/{design}/" + viewName.TrimStart('~', '/');
                }

                searchedLocations.Add(mappedPath);
                if (DoesFileExist(mappedPath))
                {
                    return mappedPath;
                }
                return null;
            }

            // Case B: Relative cross-folder path e.g. "../Products/Detail"
            if (viewName.StartsWith("../", StringComparison.Ordinal))
            {
                string relativePath = viewName.Substring(3);
                string mappedPath = $"~/Views/Designs/{design}/{relativePath}{(relativePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ? "" : ".cshtml")}";
                searchedLocations.Add(mappedPath);
                if (DoesFileExist(mappedPath))
                {
                    return mappedPath;
                }
                return null;
            }

            // Case C: Standard view probing
            List<string> probeLocations = new List<string>();

            if (!string.IsNullOrEmpty(area))
            {
                probeLocations.Add($"~/Views/Designs/{design}/Areas/{area}/{controller}/{viewNameWithExt}");
                probeLocations.Add($"~/Views/Designs/{design}/Areas/{area}/Shared/{viewNameWithExt}");
            }

            probeLocations.Add($"~/Views/Designs/{design}/{controller}/{viewNameWithExt}");
            probeLocations.Add($"~/Views/Designs/{design}/Shared/{viewNameWithExt}");

            // Also check subfolders if viewName contains slashes like "ShoppingCartTemplates/_HomePageShoppingCart"
            if (viewName.Contains("/"))
            {
                probeLocations.Add($"~/Views/Designs/{design}/{viewNameWithExt}");
            }

            foreach (var location in probeLocations)
            {
                if (!searchedLocations.Contains(location))
                {
                    searchedLocations.Add(location);
                }

                if (DoesFileExist(location))
                {
                    return location;
                }
            }

            return null;
        }

        private bool DoesFileExist(string virtualPath)
        {
            if (FileExistsOverride != null)
            {
                return FileExistsOverride(virtualPath);
            }

            if (HostingEnvironment.IsHosted && HostingEnvironment.VirtualPathProvider != null)
            {
                return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
            }

            // Fallback for non-hosted / testing contexts
            string appDomainPath = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = virtualPath.Replace("~/", "").Replace('/', Path.DirectorySeparatorChar);
            string physicalPath = Path.Combine(appDomainPath, relativePath);
            return File.Exists(physicalPath);
        }

        private static string GetAreaName(ControllerContext controllerContext)
        {
            object area;
            if (controllerContext.RouteData.DataTokens.TryGetValue("area", out area))
            {
                return area?.ToString();
            }
            return null;
        }

        private static bool IsAdminArea(string areaName)
        {
            return string.Equals(areaName, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Views that intentionally live outside storefront designs (see DesignValidator).
        /// </summary>
        private static bool IsDesignExcludedView(string controllerName, string viewName)
        {
            return string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase)
                && string.Equals(viewName, "AdminLogin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
