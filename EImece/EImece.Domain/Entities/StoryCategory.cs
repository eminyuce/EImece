using EImece.Domain.Helpers.Extensions;
using Resources;
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

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PageTheme))]
        public string PageTheme { get; set; }

        // Kept for Razor view compatibility — canonical logic lives in StorefrontCategoryDto.DetailPageAbsoluteUrl
        [NotMapped]
        public string DetailPageAbsoluteUrl
        {
            get
            {
                var rc = HttpContext.Current?.Request?.RequestContext;
                if (rc == null) return this.GetDetailPageUrl("Categories", "Stories", "", AppConfig.HttpProtocol, "");
                return new UrlHelper(rc).Action("categories", "stories", new { id = this.GetSeoUrl() }, AppConfig.HttpProtocol);
            }
        }

        // Kept for Razor view compatibility — canonical logic lives in StorefrontCategoryDto.DetailPageRelativeUrl
        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                var rc = HttpContext.Current?.Request?.RequestContext;
                if (rc == null) return this.GetDetailPageUrl("Categories", "Stories", "", "", "");
                return new UrlHelper(rc).Action("categories", "stories", new { id = this.GetSeoUrl() });
            }
        }
    }
}