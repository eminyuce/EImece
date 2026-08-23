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

        [NotMapped]
        public string DetailPageAbsoluteUrl
        {
            get
            {
                return this.GetDetailPageUrl("Categories", "Stories", "", AppConfig.HttpProtocol);
            }
        }

        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                return this.GetDetailPageUrl("Categories", "Stories");
            }
        }
    }
}