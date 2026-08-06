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
            get
            {
                if (!TryParseMenuLink(out var controller, out var action, out _))
                {
                    return "";
                }

                String result = "active";
                string resultLink = "";
                var pageAction = HtmlRequestHelper.Action();
                var pageController = HtmlRequestHelper.Controller();
                if (pageController.Equals("info", StringComparison.InvariantCultureIgnoreCase)
                    && pageAction.Equals("index", StringComparison.InvariantCultureIgnoreCase))
                {
                    var absolutePath = HttpContext.Current.Request.Url.AbsolutePath.ToString();
                    resultLink = absolutePath.ToLower().Contains(MenuLink.Replace("-", "/")) ? result : "";
                }
                else if (pageController.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
                {
                    resultLink = pageAction.Equals("detail", StringComparison.InvariantCultureIgnoreCase) ? result : "";
                }
                else if (pageController.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                                                            && pageAction.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                {
                    resultLink = pageAction.Equals(action, StringComparison.InvariantCultureIgnoreCase)
                            && pageController.Equals(controller, StringComparison.InvariantCultureIgnoreCase)
                        ? result : "";
                }
                else if (pageController.Equals("Products", StringComparison.InvariantCultureIgnoreCase)
                                                          && pageAction.Equals("detail", StringComparison.InvariantCultureIgnoreCase))
                {
                    resultLink = "";
                }
                else
                {
                    resultLink = pageAction.Equals(action, StringComparison.InvariantCultureIgnoreCase)
                                    && pageController.Equals(controller, StringComparison.InvariantCultureIgnoreCase)
                                ? result : "";
                }
                return resultLink;
            }
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
