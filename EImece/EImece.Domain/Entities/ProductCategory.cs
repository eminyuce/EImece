using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace EImece.Domain.Entities
{
    public class ProductCategory : BaseContent
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductCategoryParentId))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public Boolean MainPage { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ShortDescription))]
        public string ShortDescription { get; set; }

        public ICollection<Product> Products { get; set; }

        [ForeignKey("Template")]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TemplateId))]
        public int? TemplateId { get; set; }

        // Needed for Admin panel — category tree building for admin management screens.
        [NotMapped]
        public List<ProductCategory> Childrens { get; set; }

        // Needed for Admin panel — category tree parent reference used while building admin category hierarchy.
        [NotMapped]
        public ProductCategory Parent { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductCategoryDiscountPercantage))]
        public double? DiscountPercantage { get; set; }

        // Kept for Razor view compatibility — canonical storefront logic lives in StorefrontCategoryDto.DetailPageUrl / ProductCategoryLink
        [NotMapped]
        public string ProductCategoryLink
        {
            get
            {
                return this.GetDetailPageUrl("Category", "ProductCategories", "", "", "");
            }
        }

        // Kept for Razor view compatibility — canonical storefront logic lives in StorefrontCategoryDto.DetailPageUrl
        [NotMapped]
        public string DetailPageUrl
        {
            get
            {
                return this.GetDetailPageUrl("Category", "ProductCategories", "", "", "");
            }
        }

        public string ProductCategoryListPageUrl(SortingType sorting, IPaginatedModelList paginatedModelList)
        {
            var sortingInt = (int)sorting;
            var seoId = this.GetSeoUrl();
            var search = paginatedModelList?.Search;
            var filter = paginatedModelList?.Filter;
            var url = $"/productcategories/category/{seoId}?sorting={sortingInt}";
            if (!string.IsNullOrEmpty(search)) url += $"&search={WebUtility.UrlEncode(search)}";
            if (!string.IsNullOrEmpty(filter)) url += $"&filtreler={WebUtility.UrlEncode(filter)}";
            return url;
        }

        public Template Template { get; set; }

        // Needed for Admin panel — renders the category tree node HTML preview in admin category management.
        [NotMapped]
        public string CreateChildDataContent
        {
            get
            {
                if (MainImage != null)
                {
                    var mainImageUrl = MainImage.GetCroppedImageUrl(
                  MainImage.Id,
                  300, 0, false);
                    var result = string.Format("<img src='{0}' class='d-block mt-n1' alt='{1}'><div class='text-center font-size-sm font-weight-semibold mt-n0 pb-0'>{1}</div>", mainImageUrl,
                        this.Name);
                    return WebUtility.HtmlEncode(result);
                }
                else
                {
                    return "";
                }
            }
        }
    }
}