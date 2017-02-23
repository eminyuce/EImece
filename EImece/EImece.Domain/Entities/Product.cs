using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        [AllowHtml]
        public string VideoUrl { get; set; }



        public  ProductCategory ProductCategory { get; set; }
        public  ICollection<ProductFile> ProductFiles { get; set; }
        public  ICollection<ProductTag> ProductTags { get; set; }
        public  ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }
}
