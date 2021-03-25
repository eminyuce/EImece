using EImece.Domain.Helpers.Extensions;
using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Tag : BaseEntity
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TagCategoryId))]
        public int TagCategoryId { get; set; }

        public TagCategory TagCategory { get; set; }

        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<StoryTag> StoryTags { get; set; }

        public ICollection<FileStorageTag> FileStorageTags { get; set; }

        [NotMapped]
        public string DetailPageRelativeUrlForProducts
        {
            get
            {
                var requestContext = HttpContext.Current.Request.RequestContext;
                return new UrlHelper(requestContext).Action("Tag", "products", new { id = this.GetSeoUrl() });
            }
        }

        [NotMapped]
        public string DetailPageRelativeUrlForStories
        {
            get
            {
                var requestContext = HttpContext.Current.Request.RequestContext;
                return new UrlHelper(requestContext).Action("tag", "stories", new { id = this.GetSeoUrl() });
            }
        }
    }
}