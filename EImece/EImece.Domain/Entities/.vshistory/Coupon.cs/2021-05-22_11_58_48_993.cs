using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
     public class Coupon : BaseEntity
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TagCategoryId))]
        public int TagCategoryId { get; set; }

    }
}
