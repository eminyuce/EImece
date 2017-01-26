using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public int TagCategoryId { get; set; }
        public virtual TagCategory TagCategory { get; set; }

        public virtual ICollection<ProductTag> ProductTags { get; set; }
        public virtual ICollection<StoryTag> StoryTags { get; set; }
    }
}
