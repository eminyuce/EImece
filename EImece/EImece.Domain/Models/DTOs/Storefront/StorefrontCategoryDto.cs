using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected category read model for storefront navigation, homepage widgets, and category headers.
    /// </summary>
    public class StorefrontCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int? MainImageId { get; set; }
        public int? DiscountPercentage { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public bool MainPage { get; set; }
        public int? TemplateId { get; set; }
        public int ProductCount { get; set; }
        public int ItemCount { get { return ProductCount; } set { ProductCount = value; } }
        public int StoryCount { get { return ProductCount; } set { ProductCount = value; } }
        public int TreeLevel { get; set; }
        public string PageTheme { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public StorefrontCategoryDto Parent { get; set; }

        public List<StorefrontCategoryDto> Children { get; set; }
        public List<StorefrontStoryCardDto> Stories { get; set; }

        public StorefrontCategoryDto()
        {
            Children = new List<StorefrontCategoryDto>();
            Stories = new List<StorefrontStoryCardDto>();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }

        public string GetSeoTitle(int lang = 1)
        {
            return !string.IsNullOrWhiteSpace(MetaTitle) ? MetaTitle : Name;
        }

        public string GetSeoDescription(int lang = 1)
        {
            return !string.IsNullOrWhiteSpace(MetaDescription) ? MetaDescription : (!string.IsNullOrWhiteSpace(ShortDescription) ? ShortDescription : Description);
        }

        public string GetSeoKeywords(int lang = 1)
        {
            return !string.IsNullOrWhiteSpace(MetaKeywords) ? MetaKeywords : Name;
        }

        public string ModifiedId
        {
            get { return GeneralHelper.ModifyId(Id); }
        }

        public string SeoUrl
        {
            get { return string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(Name), ModifiedId); }
        }

        public string GetSeoUrl()
        {
            return SeoUrl;
        }

        public bool IsStoryCategory { get; set; }

        public string DetailPageUrl
        {
            get
            {
                if (IsStoryCategory)
                {
                    var dummy = new StoryCategory { Id = Id, Name = Name };
                    return dummy.GetDetailPageUrl("Categories", "Stories");
                }
                else
                {
                    var dummy = new ProductCategory { Id = Id, Name = Name };
                    return dummy.GetDetailPageUrl("Category", "ProductCategories");
                }
            }
        }

        public string DetailPageRelativeUrl
        {
            get { return DetailPageUrl; }
        }

        public string DetailPageAbsoluteUrl
        {
            get { return DetailPageUrl; }
        }

        public string StoryCategoryDetailPageUrl
        {
            get
            {
                var dummy = new StoryCategory { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Categories", "Stories");
            }
        }

        public string ProductCategoryListPageUrl(SortingType sorting, IPaginatedModelList paginatedModelList)
        {
            var dummy = new ProductCategory { Id = Id, Name = Name };
            return dummy.ProductCategoryListPageUrl(sorting, paginatedModelList);
        }

        public string GetCroppedImageUrl(int? fileStorageId = null, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = fileStorageId.HasValue ? fileStorageId.Value : (MainImageId.HasValue ? MainImageId.Value : 0);
            var dummy = new ProductCategory { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetResponsiveImageSrcSet(int fileStorageId, int width = 0, int height = 0)
        {
            var dummy = new ProductCategory { Id = Id, Name = Name };
            return dummy.GetResponsiveImageSrcSet(fileStorageId, width, height);
        }
    }
}
