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
        private const string CshtmlExtension = ".cshtml";

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
                return FindPartialViewWithoutDesign(controllerContext, partialViewName, areaName, controllerName, useCache);
            }

            List<string> searchedLocations = new List<string>();
            string resolvedPath = ResolveDesignViewPath(activeDesign, areaName, controllerName, partialViewName, searchedLocations);

            if (!string.IsNullOrEmpty(resolvedPath))
            {
                IView view = CreatePartialView(controllerContext, resolvedPath);
                return new ViewEngineResult(view, this);
            }

            if (IsBuiltInMvcTemplate(partialViewName))
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

        private ViewEngineResult FindPartialViewWithoutDesign(
            ControllerContext controllerContext,
            string partialViewName,
            string areaName,
            string controllerName,
            bool useCache)
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

        private static bool IsBuiltInMvcTemplate(string partialViewName)
        {
            return partialViewName.StartsWith("EditorTemplates/", StringComparison.OrdinalIgnoreCase) ||
                   partialViewName.StartsWith("DisplayTemplates/", StringComparison.OrdinalIgnoreCase);
        }

        private string ResolveDesignViewPath(string design, string area, string controller, string viewName, List<string> searchedLocations)
        {
            string viewNameWithExt = viewName.EndsWith(CshtmlExtension, StringComparison.OrdinalIgnoreCase)
                ? viewName
                : viewName + CshtmlExtension;

            if (IsAbsoluteViewName(viewName))
            {
                return ResolveAbsoluteDesignView(design, viewName, searchedLocations);
            }

            if (viewName.StartsWith("../", StringComparison.Ordinal))
            {
                return ResolveParentRelativeDesignView(design, viewName, searchedLocations);
            }

            return ProbeStandardDesignViewLocations(design, area, controller, viewName, viewNameWithExt, searchedLocations);
        }

        private static bool IsAbsoluteViewName(string viewName)
        {
            return viewName.StartsWith("~/", StringComparison.Ordinal) || viewName.StartsWith("/", StringComparison.Ordinal);
        }

        private string ResolveAbsoluteDesignView(string design, string viewName, List<string> searchedLocations)
        {
            string mappedPath = MapAbsoluteViewToDesignPath(design, viewName);
            searchedLocations.Add(mappedPath);
            if (DoesFileExist(mappedPath))
            {
                return mappedPath;
            }
            return null;
        }

        private static string MapAbsoluteViewToDesignPath(string design, string viewName)
        {
            if (viewName.StartsWith("~/Views/Designs/", StringComparison.OrdinalIgnoreCase))
            {
                return viewName;
            }

            if (viewName.StartsWith("~/Areas/", StringComparison.OrdinalIgnoreCase))
            {
                return MapAreasViewToDesignPath(design, viewName);
            }

            if (viewName.StartsWith("~/Views/", StringComparison.OrdinalIgnoreCase))
            {
                return $"~/Views/Designs/{design}/" + viewName.Substring("~/Views/".Length);
            }

            return $"~/Views/Designs/{design}/" + viewName.TrimStart('~', '/');
        }

        private static string MapAreasViewToDesignPath(string design, string viewName)
        {
            string pathWithoutAreas = viewName.Substring("~/Areas/".Length);
            int firstSlash = pathWithoutAreas.IndexOf('/');
            if (firstSlash > 0)
            {
                string targetArea = pathWithoutAreas.Substring(0, firstSlash);
                string rest = pathWithoutAreas.Substring(firstSlash).Replace("/Views/", "/");
                return $"~/Views/Designs/{design}/Areas/{targetArea}{rest}";
            }

            return $"~/Views/Designs/{design}/{pathWithoutAreas}";
        }

        private string ResolveParentRelativeDesignView(string design, string viewName, List<string> searchedLocations)
        {
            string relativePath = viewName.Substring(3);
            string mappedPath = $"~/Views/Designs/{design}/{relativePath}{(relativePath.EndsWith(CshtmlExtension, StringComparison.OrdinalIgnoreCase) ? "" : CshtmlExtension)}";
            searchedLocations.Add(mappedPath);
            if (DoesFileExist(mappedPath))
            {
                return mappedPath;
            }
            return null;
        }

        private string ProbeStandardDesignViewLocations(
            string design,
            string area,
            string controller,
            string viewName,
            string viewNameWithExt,
            List<string> searchedLocations)
        {
            List<string> probeLocations = new List<string>();

            if (!string.IsNullOrEmpty(area))
            {
                probeLocations.Add($"~/Views/Designs/{design}/Areas/{area}/{controller}/{viewNameWithExt}");
                probeLocations.Add($"~/Views/Designs/{design}/Areas/{area}/Shared/{viewNameWithExt}");
            }

            probeLocations.Add($"~/Views/Designs/{design}/{controller}/{viewNameWithExt}");
            probeLocations.Add($"~/Views/Designs/{design}/Shared/{viewNameWithExt}");

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
            if (!string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return string.Equals(viewName, "AdminLogin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(viewName, "VerifyAuthenticator", StringComparison.OrdinalIgnoreCase);
        }
    }
}
