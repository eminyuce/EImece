using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Net;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected brand read model for storefront filters and brand listings.
    /// </summary>
    public class StorefrontBrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool MainPage { get; set; }
        public int? MainImageId { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string MetaKeywords { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public bool ImageState
        {
            get => MainImageId.HasValue && MainImageId.Value > 0;
        }

        public static StorefrontBrandDto FromEntity(Brand b)
        {
            if (b == null) return null;
            return new StorefrontBrandDto
            {
                Id = b.Id,
                Name = b.Name,
                MainPage = b.MainPage,
                MainImageId = b.MainImageId,
                Description = b.Description,
                MetaKeywords = b.MetaKeywords,
                Position = b.Position,
                Lang = b.Lang,
                IsActive = b.IsActive,
                CreatedDate = b.CreatedDate,
                UpdatedDate = b.UpdatedDate
            };
        }

        public string ModifiedId
        {
            get { return GeneralHelper.ModifyId(Id); }
        }

        public string SeoUrl
        {
            get { return string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(Name), ModifiedId); }
        }

        public string DetailPageUrl
        {
            get
            {
                var dummy = new Brand { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Brands");
            }
        }

        public string DetailPageRelativeUrl => DetailPageUrl;
        public string DetailPageAbsoluteUrl => DetailPageUrl;

        public string GetCroppedImageUrl(int? fileStorageId = null, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = fileStorageId.HasValue ? fileStorageId.Value : (MainImageId.HasValue ? MainImageId.Value : 0);
            var dummy = new Brand { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetCroppedImageTag(int width = 0, int height = 0)
        {
            return string.Format("<img src=\"{0}\" alt=\"{1}\" />", GetCroppedImageUrl(null, width, height), WebUtility.HtmlEncode(Name));
        }

        public string GetResponsiveImageSrcSet(int width = 0, int height = 0)
        {
            int imageId = MainImageId.HasValue ? MainImageId.Value : 0;
            var dummy = new Brand { Id = Id, Name = Name };
            return dummy.GetResponsiveImageSrcSet(imageId, width, height);
        }

        public string GetProductsUrl(ProductCategory category)
        {
            if (Id <= 0 || string.IsNullOrWhiteSpace(Name)) return string.Empty;
            if (category != null)
            {
                return $"/productcategories/category/{category.GetSeoUrl()}?filtreler=b{Id}";
            }
            return $"/products/searchproducts?search={WebUtility.UrlEncode(Name)}";
        }

        public string GetProductsUrl(StorefrontCategoryDto category)
        {
            if (Id <= 0 || string.IsNullOrWhiteSpace(Name)) return string.Empty;
            if (category != null)
            {
                return $"/productcategories/category/{category.GetSeoUrl()}?filtreler=b{Id}";
            }
            return $"/products/searchproducts?search={WebUtility.UrlEncode(Name)}";
        }

        public string GetSeoTitle(int lang = 1) => Name;
        public string GetSeoDescription(int lang = 1) => !string.IsNullOrWhiteSpace(ShortDescription) ? ShortDescription : (!string.IsNullOrWhiteSpace(Description) ? Description : Name);
        public string GetSeoKeywords(int lang = 1) => !string.IsNullOrWhiteSpace(MetaKeywords) ? MetaKeywords : Name;
    }
}
