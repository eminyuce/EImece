using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class TagCategory : BaseEntity
    {

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
