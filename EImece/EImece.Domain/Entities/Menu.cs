using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Menu : BaseContent
    {
        private const string UrlPathSeparator = "/";

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MenuParentId))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public Boolean MainPage { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MenuLink))]
        public string MenuLink { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Link))]
        public string Link { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PageTheme))]
        public string PageTheme { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LinkIsActive))]
        public Boolean LinkIsActive { get; set; }

        [NotMapped]
        public List<Menu> Childrens { get; set; }

        public ICollection<MenuFile> MenuFiles { get; set; }

        /// <summary>
        /// MenuLink format is "controller-action" or "controller-action_id"
        /// (e.g. home-index, info-aboutus, stories-categories_seo-url, pages-index).
        /// </summary>
        private bool TryParseMenuLink(out string controller, out string action, out string mid)
        {
            controller = null;
            action = null;
            mid = null;

            if (string.IsNullOrWhiteSpace(MenuLink))
            {
                return false;
            }

            var segments = MenuLink.Split('_');
            var parts = segments[0].Split('-');
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                return false;
            }

            controller = parts[0];
            action = parts[1];
            mid = segments[segments.Length - 1];
            return true;
        }

        [NotMapped]
        public string IsPageActived
        {
            get { return ComputeIsPageActived(); }
        }

        private string ComputeIsPageActived()
        {
                // Match the current request URL to this menu item's own link.
                // Do NOT mark every pages/detail item active — many CMS pages share MenuLink "pages-index".
                if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.Url == null)
                {
                    return "";
                }

                var currentPath = NormalizeAppPath(HttpContext.Current.Request.Url.AbsolutePath);
                if (string.IsNullOrEmpty(currentPath))
                {
                    return "";
                }

                // External / absolute Link targets: only active when browsing that exact URL.
                if (LinkIsActive && !string.IsNullOrWhiteSpace(Link))
                {
                    if (Uri.TryCreate(Link, UriKind.Absolute, out var absolute) &&
                        string.Equals(absolute.Host, HttpContext.Current.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        return PathsMatch(currentPath, NormalizeAppPath(absolute.AbsolutePath)) ? Constants.ActiveCssClass : "";
                    }

                    if (Link.StartsWith("/", StringComparison.Ordinal))
                    {
                        return PathsMatch(currentPath, NormalizeAppPath(Link)) ? Constants.ActiveCssClass : "";
                    }

                    return "";
                }

                if (!string.IsNullOrWhiteSpace(DetailPageLink) && DetailPageLink != "#" && DetailPageLink != "#!")
                {
                    var detailPath = ToAppPath(DetailPageLink);
                    if (!string.IsNullOrEmpty(detailPath) && PathsMatch(currentPath, detailPath))
                    {
                        return Constants.ActiveCssClass;
                    }
                }

                // Route-id fallback for pages/detail when MenuLink is the generic "pages-index"
                // but the SEO slug in the URL belongs to this menu row.
                if (!TryParseMenuLink(out var controller, out var action, out var mid))
                {
                    return "";
                }

                var pageController = HtmlRequestHelper.Controller();
                var pageAction = HtmlRequestHelper.Action();
                var routeId = HtmlRequestHelper.Id() ?? string.Empty;

                if (pageController.Equals("pages", StringComparison.InvariantCultureIgnoreCase)
                    && pageAction.Equals("detail", StringComparison.InvariantCultureIgnoreCase)
                    && controller.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
                {
                    var seo = this.GetSeoUrl() ?? string.Empty;
                    if (!string.IsNullOrEmpty(seo)
                        && (routeId.Equals(seo, StringComparison.OrdinalIgnoreCase)
                            || currentPath.IndexOf("/" + seo.Trim('/'), StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return Constants.ActiveCssClass;
                    }

                    return "";
                }

                if (pageController.Equals("info", StringComparison.InvariantCultureIgnoreCase)
                    && pageAction.Equals("index", StringComparison.InvariantCultureIgnoreCase)
                    && controller.Equals("info", StringComparison.InvariantCultureIgnoreCase))
                {
                    var infoKey = "/" + MenuLink.Replace("-", "/").Trim('/').ToLowerInvariant();
                    return PathsMatch(currentPath, infoKey) || currentPath.StartsWith(infoKey + "/", StringComparison.OrdinalIgnoreCase)
                        ? Constants.ActiveCssClass
                        : "";
                }

                if (pageController.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                    && controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (pageAction.Equals("categories", StringComparison.InvariantCultureIgnoreCase)
                        && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(mid) || routeId.Equals(mid, StringComparison.OrdinalIgnoreCase))
                        {
                            return Constants.ActiveCssClass;
                        }

                        return "";
                    }

                    if (pageAction.Equals(action, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Constants.ActiveCssClass;
                    }
                }

                // Generic controller/action match only when MenuLink is unique enough
                // (not the shared pages-index bucket used by many CMS pages).
                if (!MenuLink.Equals("pages-index", StringComparison.OrdinalIgnoreCase)
                    && pageController.Equals(controller, StringComparison.InvariantCultureIgnoreCase)
                    && pageAction.Equals(action, StringComparison.InvariantCultureIgnoreCase)
                    && !pageController.Equals("products", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Constants.ActiveCssClass;
                }

                return "";
        }

        private static string ToAppPath(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                return "";
            }

            if (Uri.TryCreate(href, UriKind.Absolute, out var absolute))
            {
                return NormalizeAppPath(absolute.AbsolutePath);
            }

            // Virtual app-relative (~/) or site-relative path
            var path = href;
            var q = path.IndexOfAny(new[] { '?', '#' });
            if (q >= 0)
            {
                path = path.Substring(0, q);
            }

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }

            return NormalizeAppPath(path);
        }

        private static string NormalizeAppPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return UrlPathSeparator;
            }

            path = path.Trim().Replace('\\', '/');
            if (!path.StartsWith(UrlPathSeparator, StringComparison.Ordinal))
            {
                path = UrlPathSeparator + path;
            }

            // Collapse duplicate slashes and trim trailing slash (except root)
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }

            if (path.Length > 1 && path.EndsWith("/", StringComparison.Ordinal))
            {
                path = path.TrimEnd('/');
            }

            return path.ToLowerInvariant();
        }

        private static bool PathsMatch(string currentPath, string candidatePath)
        {
            if (string.IsNullOrEmpty(currentPath) || string.IsNullOrEmpty(candidatePath))
            {
                return false;
            }

            return currentPath.Equals(candidatePath, StringComparison.OrdinalIgnoreCase);
        }

        [NotMapped]
        public string DetailPageLink
        {
            get
            {
                if (LinkIsActive && !String.IsNullOrEmpty(Link))
                {
                    return Link;
                }

                if (!TryParseMenuLink(out var controller, out var action, out var mid))
                {
                    return "#";
                }

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                if (controller.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
                {
                    return urlHelper.Action("detail", controller, new { id = this.GetSeoUrl() });
                }

                if (controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                    && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                {
                    return urlHelper.Action(action, controller, new { id = mid });
                }

                return urlHelper.Action(action, controller);
            }
        }
    }
}
