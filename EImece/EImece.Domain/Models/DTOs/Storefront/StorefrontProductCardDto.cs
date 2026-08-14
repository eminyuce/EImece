using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Purpose-specific read model for storefront product cards (listings, homepage, search, tags, related).
    /// Projected directly via LINQ with AsNoTracking() to minimize columns, joins, and materialization.
    /// </summary>
    public class StorefrontProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameShort { get; set; }
        public string NameLong { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string ProductCode { get; set; }
        public double Rating { get; set; }
        public int SoldCount { get; set; }
        public int? MainImageId { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int? BrandId { get; set; }
        public string BrandName { get; set; }
        public bool IsActive { get; set; }
        public bool MainPage { get; set; }
        public bool IsCampaign { get; set; }
        public string State { get; set; }
        public int Lang { get; set; }
        public int Position { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Computed properties (client-side derived from projected columns)
        public decimal PriceWithDiscount
        {
            get
            {
                if (Discount.HasValue && Discount.Value > 0)
                {
                    return Price - Discount.Value;
                }
                return Price;
            }
        }

        public bool HasDiscount
        {
            get { return Discount.HasValue && Discount.Value > 0; }
        }

        public int DiscountPercentage
        {
            get
            {
                if (Price > 0 && Discount.HasValue && Discount.Value > 0)
                {
                    return (int)Math.Round((Discount.Value / Price) * 100);
                }
                return 0;
            }
        }

        public bool IsBuyableState
        {
            get
            {
                return State == ProductState.ProductInStock.ToString() ||
                       State == ProductState.LimitedStock.ToString();
            }
        }

        public string ModifiedId
        {
            get { return GeneralHelper.ModifyId(Id); }
        }

        public string SeoUrl
        {
            get { return string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(Name), ModifiedId); }
        }

        public string ProductCategorySeoUrl
        {
            get
            {
                if (string.IsNullOrEmpty(ProductCategoryName)) return string.Empty;
                return string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(ProductCategoryName), GeneralHelper.ModifyId(ProductCategoryId));
            }
        }

        public string DetailPageUrl
        {
            get
            {
                var dummy = new Product { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Products", ProductCategoryName ?? "no_category");
            }
        }

        public string DetailPageRelativeUrl
        {
            get
            {
                var dummy = new Product { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Products", ProductCategoryName ?? "no_category");
            }
        }

        public string DetailPageAbsoluteUrl
        {
            get { return DetailPageUrl; }
        }

        public string ProductCategoryDetailPageUrl
        {
            get
            {
                if (ProductCategoryId <= 0) return string.Empty;
                var dummy = new ProductCategory { Id = ProductCategoryId, Name = ProductCategoryName };
                return dummy.GetDetailPageUrl("Category", "ProductCategories");
            }
        }

        public string BrandProductsUrl
        {
            get
            {
                if (!BrandId.HasValue || BrandId.Value <= 0 || string.IsNullOrWhiteSpace(BrandName))
                {
                    return string.Empty;
                }

                if (ProductCategoryId > 0)
                {
                    var catSeo = !string.IsNullOrEmpty(ProductCategorySeoUrl)
                        ? ProductCategorySeoUrl
                        : string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(ProductCategoryName ?? "kategori"), GeneralHelper.ModifyId(ProductCategoryId));
                    return $"/{Constants.ProductsCategoriesControllerRoutingPrefix}/pc/{catSeo}?filtreler=b{BrandId.Value}";
                }

                return $"/products/searchproducts?search={Uri.EscapeDataString(BrandName)}";
            }
        }

        public string GetCroppedImageUrl(int? fileStorageId, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = fileStorageId.HasValue ? fileStorageId.Value : (MainImageId.HasValue ? MainImageId.Value : 0);
            var dummy = new Product { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetCroppedImageTag(int width = 0, int height = 0, bool lazy = true, string fetchPriority = null, string sizes = null)
        {
            int imageId = MainImageId.HasValue ? MainImageId.Value : 0;
            var dummy = new Product { Id = Id, Name = Name };
            return dummy.GetCroppedImageTag(imageId, width, height, lazy: lazy, fetchPriority: fetchPriority, sizes: sizes);
        }

        public string GetResponsiveImageSrcSet(int fileStorageId, int width = 0, int height = 0)
        {
            var dummy = new Product { Id = Id, Name = Name };
            return dummy.GetResponsiveImageSrcSet(fileStorageId, width, height);
        }

        public string ImageFullPath(int width = 0, int height = 0)
        {
            return GetCroppedImageUrl(MainImageId, width, height, true);
        }
    }
}
