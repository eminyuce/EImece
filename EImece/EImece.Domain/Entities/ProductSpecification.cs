using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductSpecification : BaseEntity
    {
        public string Value { get; set; }
        public string Unit { get; set; }
        public int ProductId { get; set; }
    }
}
