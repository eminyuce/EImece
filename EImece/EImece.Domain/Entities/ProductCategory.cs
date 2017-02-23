using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class ProductCategory : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryParentId))]
        public int ParentId { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }
        public  ICollection<Product> Products { get; set; }

        [ForeignKey("Template")]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TemplateId))]
        public int? TemplateId { get; set; }
        [NotMapped]
        public List<ProductCategory> Childrens { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryDiscountPercantage))]
        public double? DiscountPercantage { get; set; }
        public  Template Template { get; set; }
    }
}
