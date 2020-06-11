using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class Tag : BaseEntity
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TagCategoryId))]
        public int TagCategoryId { get; set; }
        public TagCategory TagCategory { get; set; }

        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<StoryTag> StoryTags { get; set; }

        public ICollection<FileStorageTag> FileStorageTags { get; set; }
    }
}
