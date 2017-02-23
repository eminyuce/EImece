using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace EImece.Domain.Entities
{
    public class Tag : BaseEntity
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TagCategoryId))]
        public int TagCategoryId { get; set; }
        public  TagCategory TagCategory { get; set; }

        public  ICollection<ProductTag> ProductTags { get; set; }
        public  ICollection<StoryTag> StoryTags { get; set; }
    }
}
