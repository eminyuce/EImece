using EImece.Domain.Helpers;
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
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductName))]
        public override string Name { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.NameShort))]
        public string NameShort { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductNameLong))]
        public string NameLong { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ProductCategoryIdErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductCategoryId))]
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Brands))]
        public int? BrandId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public Boolean MainPage { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductShortDescription))]
        public string ShortDescription { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Price))]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Price))]
        [DataType(DataType.Currency)]
        public decimal? Discount { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Price))]
        public String PriceStr { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductDiscount))]
        public String DiscountStr { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ProductCodeErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductCode))]
        public string ProductCode { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.VideoUrl))]
        public string VideoUrl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsCampaign))]
        public Boolean IsCampaign { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductColorOptions))]
        public String ProductColorOptions { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductSizeOptions))]
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

        public string ImageFullPath(int width, int height)
        {
            var result= this.GetCroppedImageUrl(this.MainImageId, width, height, true);

            return result;
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
        public string BuyNowRelativeUrl
        {
            get
            {
                return this.GetDetailPageUrl("BuyNow", "Payment", ProductCategory.Name);
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
        public int DiscountPercentage
        {
            get
            {
                if (Price > 0)
                {
                    var discountedDiff = Price - PriceWithDiscount;
                    var result = Math.Round(discountedDiff * 100 / Price, 2);
                    return (int)result;
                }
                return 0;
            }
        }

        [NotMapped]
        public string ModifiedId { get { return GeneralHelper.ModifyId(Id); } }

        [NotMapped]
        public byte[] MainImageBytes { get; set; }

        [NotMapped]
        public Tuple<string, string> MainImageSrc { get; set; }

        [NotMapped]
        public decimal PriceWithDiscount
        {
            get
            {
                if (HasDiscount)
                {
                    ProductCategory productCategory = ProductCategory;
                    var categoryDiscount = productCategory.DiscountPercantage.HasValue ? (decimal)ProductCategory.DiscountPercantage.Value / 100 : 0;
                    return Price - (Discount.HasValue ? Discount.Value : 0) - (Price * (categoryDiscount));
                }
                else
                {
                    return Price;
                }
            }
        }
    }
}