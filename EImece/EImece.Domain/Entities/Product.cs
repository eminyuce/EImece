using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Product : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.NameShort))]
        public string NameShort { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.ProductCategoryIdErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryId))]
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ShortDescription))]
        public string ShortDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Price))]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Discount))]
        public double Discount { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.ProductCodeErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCode))]
        public string ProductCode { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.VideoUrl))]
        public string VideoUrl { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.IsCampaign))]
        public Boolean IsCampaign { get; set; }

        public ProductCategory ProductCategory { get; set; }
        public ICollection<ProductFile> ProductFiles { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }
}