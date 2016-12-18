using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class TagCategory : BaseEntity
    {
        public int TagType { get; set; }
        public int Lang { get; set; }
    }
}
