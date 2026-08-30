using EImece.Domain.Helpers.Extensions;
using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        // Needed for Admin panel — tag listing grid shows associated item count.
        [NotMapped]
        public int ItemCount { get; set; }

        // Kept for Razor view compatibility — canonical logic lives in StorefrontTagDto.DetailPageRelativeUrl / StoryTagDetailPageUrl
        [NotMapped]
        public string DetailPageRelativeUrlForProducts
        {
            get
            {
                return this.GetDetailPageUrl("Tag", "Products", "", "", "");
            }
        }

        // Kept for Razor view compatibility — canonical logic lives in StorefrontTagDto.StoryTagDetailPageUrl
        [NotMapped]
        public string DetailPageRelativeUrlForStories
        {
            get
            {
                return this.GetDetailPageUrl("Tag", "Stories", "", "", "");
            }
        }
    }
}