using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
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

        // Needed for Admin panel — admin product form inputs bind string values for price/discount parsing.
        [NotMapped]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Price))]
        public String PriceStr { get; set; }

        // Needed for Admin panel — admin product form inputs bind string values for price/discount parsing.
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

        // Needed for Admin panel — enum wrapper for State string is used by admin product state dropdowns and sorting.
        [NotMapped] // Prevents EF from mapping directly
        public ProductState StateEnum
        {
            get => Enum.TryParse(State, out ProductState result) ? result : ProductState.NONE;
            set => State = value.ToString();
        }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SelectProductState))]
        public string State { get; set; } // Store as VARCHAR(50)

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductSizeOptions))]
        public String ProductSizeOptions { get; set; }

        public ProductCategory ProductCategory { get; set; }
        public Brand Brand { get; set; }
        public ICollection<ProductComment> ProductComments { get; set; }
        public ICollection<ProductFile> ProductFiles { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<ProductSpecification> ProductSpecifications { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double Rating { get; set; }

        /// <summary>
        /// Total sold quantity from orders. Populated for bestseller sorting; not mapped to DB.
        /// </summary>
        // Needed for Admin panel — admin product lists can sort/filter by bestseller sold count.
        [NotMapped]
        public int SoldCount { get; set; }

        public string ImageFullPath(int width, int height, bool isThump=false)
        {
            // Must tolerate null HttpContext.Current after ConfigureAwait(false) in async services.
            var baseurl = EntityExtension.GetAbsoluteApplicationBaseUrl();
            var result = this.GetCroppedImageUrl(this.MainImageId, width, height, true, isThump) ?? string.Empty;
            if (!string.IsNullOrEmpty(baseurl) && !result.Contains(baseurl))
            {
                result = baseurl + result;
            }
            return result;
        }

        // Needed for Admin panel — admin product-comment and order detail screens link to the storefront product page.
        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                return this.GetDetailPageUrl("Detail", "Products", ProductCategory != null ? ProductCategory.Name : "no_category");
            }
        }

        // Needed for Admin panel — admin price grid shows discounted vs original price consistently with storefront logic.
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

        // Needed for Admin panel — admin price grid displays discount percentage alongside the price.
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

        // Needed for Admin panel — admin product image upload stores transient bytes before persisting to FileStorage.
        [NotMapped]
        public byte[] MainImageBytes { get; set; }

        // Needed for Admin panel — admin product form preview resolves the image src tuple before save.
        [NotMapped]
        public Tuple<string, string> MainImageSrc { get; set; }

        // Needed for Admin panel — admin price grid shows the discounted price with category discount applied.
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