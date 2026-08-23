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

        /// <summary>
        /// Active product + story associations. Populated by tag listing queries; not mapped to the DB.
        /// </summary>
        [NotMapped]
        public int ItemCount { get; set; }

        [NotMapped]
        public string DetailPageRelativeUrlForProducts
        {
            get
            {
                return this.GetDetailPageUrl("Tag", "Products");
            }
        }

        [NotMapped]
        public string DetailPageRelativeUrlForStories
        {
            get
            {
                return this.GetDetailPageUrl("Tag", Constants.StoriesAction);
            }
        }
    }
}