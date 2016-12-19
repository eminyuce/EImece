using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Product: BaseContent
    {
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please enter category")]
        public int ProductCategoryId { get; set; }

        public Boolean MainPage { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public string ProductCode { get; set; }
        public int TemplateId { get; set; }



        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ICollection<ProductFile> ProductFiles { get; set; }
        public virtual ICollection<ProductTag> ProductTags { get; set; }
    }
}
