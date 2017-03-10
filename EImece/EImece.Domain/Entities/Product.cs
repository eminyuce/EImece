using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Resources;

namespace EImece.Domain.Entities
{
    public class Product: BaseContent
    {

        public string NameShort { get; set; }
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.ProductCategoryIdErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryId))]
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Price))]
        public double Price { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Discount))]
        public double Discount { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCode))]
        public string ProductCode { get; set; }
        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.VideoUrl))]
        public string VideoUrl { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.IsCampaign))]
        public Boolean IsCampaign { get; set; }

        public  ProductCategory ProductCategory { get; set; }
        public  ICollection<ProductFile> ProductFiles { get; set; }
        public  ICollection<ProductTag> ProductTags { get; set; }
        public  ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }
}
