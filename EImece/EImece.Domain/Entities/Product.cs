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
        [NotMapped]
        public int SoldCount { get; set; }

        [NotMapped]
        public string DetailPageAbsoluteUrl
        {
            get
            {
                return this.GetDetailPageUrl("Detail", "Products", ProductCategory != null ? ProductCategory.Name : "no_category", AppConfig.HttpProtocol);
            }
        }

        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                return this.GetDetailPageUrl("Detail", "Products", ProductCategory != null ? ProductCategory.Name : "no_category");
            }
        }

        [NotMapped]
        public bool HasDiscount
        {
            get
            {
                var hasDirectDiscount = Discount.HasValue && Discount.Value > 0;
                var hasCategoryDiscount = ProductCategory != null && ProductCategory.DiscountPercantage.HasValue && ProductCategory.DiscountPercantage.Value > 0;
                return hasDirectDiscount || hasCategoryDiscount;
            }
        }

        [NotMapped]
        public string ProductNameStr
        {
            get
            {
                if (!string.IsNullOrEmpty(NameShort))
                {
                    return NameShort;
                }
                if (!string.IsNullOrEmpty(NameLong))
                {
                    return NameLong;
                }
                return Name;
            }
        }

        [NotMapped]
        public decimal PriceWithDiscount
        {
            get
            {
                if (HasDiscount)
                {
                    ProductCategory productCategory = ProductCategory;
                    var categoryDiscount = productCategory != null && productCategory.DiscountPercantage.HasValue ? (decimal)productCategory.DiscountPercantage.Value / 100 : 0;
                    return Price - (Discount.HasValue ? Discount.Value : 0) - (Price * (categoryDiscount));
                }
                else
                {
                    return Price;
                }
            }
        }

        [NotMapped]
        public double DiscountPercentage
        {
            get
            {
                if (!HasDiscount)
                {
                    return 0;
                }
                if (Discount.HasValue && Discount.Value > 0 && Price > 0)
                {
                    return (double)((Discount.Value / Price) * 100);
                }
                if (ProductCategory != null && ProductCategory.DiscountPercantage.HasValue)
                {
                    return ProductCategory.DiscountPercantage.Value;
                }
                return 0;
            }
        }

        [NotMapped]
        public string ModifiedId
        {
            get
            {
                return GeneralHelper.ModifyId(this.Id);
            }
        }

        [NotMapped]
        public bool IsBuyableState
        {
            get
            {
                if (Price <= 0)
                {
                    return false;
                }
                switch (StateEnum)
                {
                    case ProductState.ProductInStock:
                    case ProductState.PreOrder:
                    case ProductState.LimitedStock:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True when the product has a price and is available for sale
        /// (in stock, pre-order, limited stock, or coming soon).
        /// Used for the "Satışta" listing badge.
        /// </summary>
        [NotMapped]
        public bool IsOnSale
        {
            get
            {
                if (Price <= 0)
                {
                    return false;
                }

                switch (StateEnum)
                {
                    case ProductState.ProductInStock:
                    case ProductState.PreOrder:
                    case ProductState.LimitedStock:
                    case ProductState.ComingSoon:
                        return true;
                    default:
                        return false;
                }
            }
        }

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
    }
}