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
        public int ParentId { get; set; }
        public Boolean MainPage { get; set; }
        public  ICollection<Product> Products { get; set; }

        [ForeignKey("Template")]
        public int? TemplateId { get; set; }
        [NotMapped]
        public List<ProductCategory> Childrens { get; set; }

        public double? DiscountPercantage { get; set; }
        public  Template Template { get; set; }
    }
}
