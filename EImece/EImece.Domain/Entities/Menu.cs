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
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MenuParentId))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MenuLink))]
        public string MenuLink { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Static))]
        public Boolean Static { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Link))]
        public string Link { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PageTheme))]
        public string PageTheme { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LinkIsActive))]
        public Boolean LinkIsActive { get; set; }

        [NotMapped]
        public List<Menu> Childrens { get; set; }

        public ICollection<MenuFile> MenuFiles { get; set; }

        [NotMapped]
        public string DetailPageLink
        {
            get
            {

                var p = MenuLink.Split("_".ToCharArray());
                var parts = p.First().Split("-".ToCharArray());
                var action = parts[1];
                var controller = parts[0];
                String mid =  p.Last();
                string resultLink = "";
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                if (LinkIsActive && !String.IsNullOrEmpty(Link))
                {
                    resultLink = Link;
                }
                else if (controller.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
                {
                    resultLink = urlHelper.Action("detail", controller, new { id = this.GetSeoUrl() });
                }
                else if (controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                                                            && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                {
                    resultLink = urlHelper.Action(action, controller, new { id = mid });
                }
                else
                {
                    resultLink = urlHelper.Action(action, controller);
                }
                return resultLink;
            }
        }
    }
}