using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class StoryCategory : BaseContent
    {
        public ICollection<Story> Stories { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PageTheme))]
        public String PageTheme { get; set; }

        [NotMapped]
        public string DetailPageAbsoluteUrl
        {
            get
            {
                var requestContext = HttpContext.Current.Request.RequestContext;
                return new UrlHelper(requestContext).Action("categories", "stories", new { id = this.GetSeoUrl() }, AppConfig.HttpProtocol);
            }
        }

        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                var requestContext = HttpContext.Current.Request.RequestContext;
                return new UrlHelper(requestContext).Action("categories", "stories", new { id = this.GetSeoUrl() });
            }
        }
    }
}