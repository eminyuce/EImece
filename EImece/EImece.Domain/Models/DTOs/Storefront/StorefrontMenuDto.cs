using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected menu read model for navigation, header, and footer.
    /// </summary>
    public class StorefrontMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string MenuLink { get; set; }
        public string Url { get; set; }
        public string Link
        {
            get => Url;
            set => Url = value;
        }
        public bool LinkIsActive { get; set; }
        public string Target { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public int? MainImageId { get; set; }
        public bool ImageState
        {
            get => MainImageId.HasValue && MainImageId.Value > 0;
        }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public bool MainPage { get; set; }
        public int TreeLevel { get; set; }
        public string PageTheme { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public List<StorefrontMenuDto> Childrens
        {
            get => Children;
            set => Children = value;
        }

        private const string UrlPathSeparator = "/";

        /// <summary>
        /// Request-aware active-state detection, identical to Menu.IsPageActived, evaluated
        /// on the projected DTO so navigation trees do not need entity materialization.
        /// </summary>
        public string IsPageActived
        {
            get { return ComputeIsPageActived(); }
        }

        private string ComputeIsPageActived()
        {
            var currentPath = TryGetCurrentAppPath();
            if (string.IsNullOrEmpty(currentPath))
            {
                return "";
            }

            string linkResult;
            if (TryMatchActiveLink(currentPath, out linkResult))
            {
                return linkResult;
            }

            if (MatchesDetailPageLink(currentPath))
            {
                return Constants.ActiveCssClass;
            }

            string controller;
            string action;
            string mid;
            if (!TryParseMenuLink(out controller, out action, out mid))
            {
                return "";
            }

            var pageController = HtmlRequestHelper.Controller();
            var pageAction = HtmlRequestHelper.Action();
            var routeId = HtmlRequestHelper.Id() ?? string.Empty;

            string routeResult;
            if (TryMatchPagesDetail(pageController, pageAction, controller, routeId, currentPath, out routeResult))
            {
                return routeResult;
            }

            if (TryMatchInfoIndex(pageController, pageAction, controller, currentPath, out routeResult))
            {
                return routeResult;
            }

            if (TryMatchStories(pageController, pageAction, controller, action, mid, routeId, out routeResult))
            {
                return routeResult;
            }

            if (MatchesGenericControllerAction(pageController, pageAction, controller, action))
            {
                return Constants.ActiveCssClass;
            }

            return "";
        }

        private static string TryGetCurrentAppPath()
        {
            if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.Url == null)
            {
                return "";
            }

            return NormalizeAppPath(System.Web.HttpContext.Current.Request.Url.AbsolutePath);
        }

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

        private bool TryMatchActiveLink(string currentPath, out string result)
        {
            result = "";
            if (!LinkIsActive || string.IsNullOrWhiteSpace(Url))
            {
                return false;
            }

            if (Uri.TryCreate(Url, UriKind.Absolute, out var absolute) &&
                string.Equals(absolute.Host, System.Web.HttpContext.Current.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
            {
                result = PathsMatch(currentPath, NormalizeAppPath(absolute.AbsolutePath)) ? Constants.ActiveCssClass : "";
                return true;
            }

            if (Url.StartsWith("/", StringComparison.Ordinal))
            {
                result = PathsMatch(currentPath, NormalizeAppPath(Url)) ? Constants.ActiveCssClass : "";
                return true;
            }

            return true;
        }

        private bool MatchesDetailPageLink(string currentPath)
        {
            var detailPageLink = DetailPageUrl;
            if (string.IsNullOrWhiteSpace(detailPageLink) || detailPageLink == "#" || detailPageLink == "#!")
            {
                return false;
            }

            var detailPath = ToAppPath(detailPageLink);
            return !string.IsNullOrEmpty(detailPath) && PathsMatch(currentPath, detailPath);
        }

        private bool TryMatchPagesDetail(
            string pageController,
            string pageAction,
            string controller,
            string routeId,
            string currentPath,
            out string result)
        {
            result = "";
            if (!pageController.Equals("pages", StringComparison.InvariantCultureIgnoreCase)
                || !pageAction.Equals("detail", StringComparison.InvariantCultureIgnoreCase)
                || !controller.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var seo = this.GetSeoUrl() ?? string.Empty;
            if (!string.IsNullOrEmpty(seo)
                && (routeId.Equals(seo, StringComparison.OrdinalIgnoreCase)
                    || currentPath.IndexOf("/" + seo.Trim('/'), StringComparison.OrdinalIgnoreCase) >= 0))
            {
                result = Constants.ActiveCssClass;
            }

            return true;
        }

        private bool TryMatchInfoIndex(string pageController, string pageAction, string controller, string currentPath, out string result)
        {
            result = "";
            if (!pageController.Equals("info", StringComparison.InvariantCultureIgnoreCase)
                || !pageAction.Equals("index", StringComparison.InvariantCultureIgnoreCase)
                || !controller.Equals("info", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var infoKey = "/" + MenuLink.Replace("-", "/").Trim('/').ToLowerInvariant();
            result = PathsMatch(currentPath, infoKey) || currentPath.StartsWith(infoKey + "/", StringComparison.OrdinalIgnoreCase)
                ? Constants.ActiveCssClass
                : "";
            return true;
        }

        private static bool TryMatchStories(
            string pageController,
            string pageAction,
            string controller,
            string action,
            string mid,
            string routeId,
            out string result)
        {
            result = "";
            if (!pageController.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                || !controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (pageAction.Equals("categories", StringComparison.InvariantCultureIgnoreCase)
                && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(mid) || routeId.Equals(mid, StringComparison.OrdinalIgnoreCase))
                {
                    result = Constants.ActiveCssClass;
                }

                return true;
            }

            if (pageAction.Equals(action, StringComparison.InvariantCultureIgnoreCase))
            {
                result = Constants.ActiveCssClass;
                return true;
            }

            return false;
        }

        private bool MatchesGenericControllerAction(string pageController, string pageAction, string controller, string action)
        {
            return !MenuLink.Equals("pages-index", StringComparison.OrdinalIgnoreCase)
                && pageController.Equals(controller, StringComparison.InvariantCultureIgnoreCase)
                && pageAction.Equals(action, StringComparison.InvariantCultureIgnoreCase)
                && !pageController.Equals("products", StringComparison.InvariantCultureIgnoreCase);
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

        public List<StorefrontMenuDto> Children { get; set; }
        public List<StorefrontMenuDto> SideMenus { get; set; }
        public List<StorefrontMenuFileDto> MenuFiles { get; set; }

        public StorefrontMenuDto()
        {
            Children = new List<StorefrontMenuDto>();
            SideMenus = new List<StorefrontMenuDto>();
            MenuFiles = new List<StorefrontMenuFileDto>();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }

        public static StorefrontMenuDto FromEntity(Menu m)
        {
            if (m == null) return null;
            return new StorefrontMenuDto
            {
                Id = m.Id,
                Name = m.Name,
                ParentId = m.ParentId,
                MenuLink = m.MenuLink,
                Url = m.Link,
                LinkIsActive = m.LinkIsActive,
                ShortDescription = m.Description,
                Description = m.Description,
                MetaKeywords = m.MetaKeywords,
                MainImageId = m.MainImageId,
                Position = m.Position,
                Lang = m.Lang,
                IsActive = m.IsActive,
                MainPage = m.MainPage,
                PageTheme = m.PageTheme,
                CreatedDate = m.CreatedDate,
                UpdatedDate = m.UpdatedDate
            };
        }

        public string ModifiedId
        {
            get { return GeneralHelper.ModifyId(Id); }
        }

        public string SeoUrl
        {
            get { return string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(Name), ModifiedId); }
        }

        public string GetSeoUrl()
        {
            return SeoUrl;
        }

        public string GetSeoTitle(int lang = 1)
        {
            return Name;
        }

        public string GetSeoDescription(int lang = 1)
        {
            return !string.IsNullOrWhiteSpace(ShortDescription) ? ShortDescription : (!string.IsNullOrWhiteSpace(Description) ? Description : Name);
        }

        public string GetSeoKeywords(int lang = 1)
        {
            return Name;
        }

        public string DetailPageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(Url)) return Url;
                var dummy = new Menu { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Pages");
            }
        }

        public string DetailPageRelativeUrl
        {
            get { return DetailPageUrl; }
        }

        public string DetailPageAbsoluteUrl
        {
            get { return DetailPageUrl; }
        }

        public string DetailPageLink
        {
            get { return DetailPageUrl; }
        }

        public string GetCroppedImageUrl(int? fileStorageId = null, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = fileStorageId.HasValue ? fileStorageId.Value : (MainImageId.HasValue ? MainImageId.Value : 0);
            var dummy = new Menu { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetCroppedImageTag(int width = 0, int height = 0)
        {
            return string.Format("<img src=\"{0}\" alt=\"{1}\" />", GetCroppedImageUrl(null, width, height), System.Web.HttpUtility.HtmlAttributeEncode(Name));
        }
    }
}
