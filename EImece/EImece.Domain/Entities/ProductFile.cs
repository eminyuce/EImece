using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductFile : BaseEntity
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
    }
}
