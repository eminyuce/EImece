using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Product : BaseContent
    {

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductName))]
        public override string Name { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.NameShort))]
        public string NameShort { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductNameLong))]
        public string NameLong { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.ProductCategoryIdErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryId))]
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Brands))]
        public int? BrandId { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductShortDescription))]
        public string ShortDescription { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Price))]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductDiscount))]
        public double Discount { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.ProductCodeErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCode))]
        public string ProductCode { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.VideoUrl))]
        public string VideoUrl { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.IsCampaign))]
        public Boolean IsCampaign { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductColorOptions))]
        public String ProductColorOptions { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductSizeOptions))]
        public String ProductSizeOptions { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public ICollection<ProductComment> ProductComments { get; set; }
        public ICollection<ProductFile> ProductFiles { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<ProductSpecification> ProductSpecifications { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double Rating { get; set; }

        [NotMapped]
        public string DetailPageAbsoluteUrl
        {
            get
            {
                return this.GetDetailPageUrl("Detail", "Products", ProductCategory.Name, AppConfig.HttpProtocol);
            }
        }
        public string ProductImageFullPath(int width,int height)
        {
           return this.GetCroppedImageUrl(this.MainImageId, width, height, true);
        }



        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                return this.GetDetailPageUrl("Detail", "Products", ProductCategory.Name);
            }
        }

        [NotMapped]
        public bool HasDiscount
        {
            get
            {
                if (ProductCategory == null)
                {
                    return false;
                }
                var hasCategoryDiscount = ProductCategory.DiscountPercantage.HasValue && ProductCategory.DiscountPercantage.Value > 0;
                if (hasCategoryDiscount || Discount > 0)
                {
                    return true;
                }
                return false;
            }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return ((Product)obj).Id == this.Id;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [NotMapped]
        public double DiscountPercentage
        {
            get
            {
                double discountedDiff = Price - PriceWithDiscount;
                var result = Math.Round(discountedDiff * 100 / Price, 2);
                return result;
            }
        }

        [NotMapped]
        public double PriceWithDiscount
        {
            get
            {
                if (HasDiscount)
                {
                    ProductCategory productCategory = ProductCategory;
                    var categoryDiscount = productCategory.DiscountPercantage.HasValue ? ProductCategory.DiscountPercantage.Value : 0;
                    return Price - (Discount) - (Price * (categoryDiscount / 100));
                }
                else
                {
                    return Price;
                }
            }
        }
    }
}