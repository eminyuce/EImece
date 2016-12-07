using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductCategory : BaseContent
    {
        public int ? ParentId { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        [NotMapped]
        public List<ProductCategory> Childrens { get; set; }
    }
}
