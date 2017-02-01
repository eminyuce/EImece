using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Product: BaseContent
    {
     
        [Display(Name = "Selected Category")]
        [Required(ErrorMessage = "Please enter Category")]
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }

        public Boolean MainPage { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public string ProductCode { get; set; }
        public string VideoUrl { get; set; }



        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ICollection<ProductFile> ProductFiles { get; set; }
        public virtual ICollection<ProductTag> ProductTags { get; set; }
        public virtual ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }
}
