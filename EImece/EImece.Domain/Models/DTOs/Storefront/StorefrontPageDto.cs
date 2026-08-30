using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected CMS page read model for pages/info detail views.
    /// </summary>
    public class StorefrontPageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MenuLink { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public int? MainImageId { get; set; }
        public string MetaKeywords { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public string PageTheme { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public bool ImageState
        {
            get => MainImageId.HasValue && MainImageId.Value > 0;
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
                var dummy = new Menu { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Pages");
            }
        }

        public string DetailPageRelativeUrl => DetailPageUrl;
        public string DetailPageAbsoluteUrl => DetailPageUrl;
        public string DetailPageLink => DetailPageUrl;

        public string GetCroppedImageUrl(int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = MainImageId.HasValue ? MainImageId.Value : 0;
            var dummy = new Menu { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetCroppedImageTag(int width = 0, int height = 0)
        {
            return string.Format("<img src=\"{0}\" alt=\"{1}\" />", GetCroppedImageUrl(width, height), System.Net.WebUtility.HtmlEncode(Name));
        }

        public string GetResponsiveImageSrcSet(int width = 0, int height = 0)
        {
            int imageId = MainImageId.HasValue ? MainImageId.Value : 0;
            var dummy = new Menu { Id = Id, Name = Name };
            return dummy.GetResponsiveImageSrcSet(imageId, width, height);
        }

        public string GetSeoTitle(int lang = 1) => Name;
        public string GetSeoDescription(int lang = 1) => !string.IsNullOrWhiteSpace(ShortDescription) ? ShortDescription : (!string.IsNullOrWhiteSpace(Description) ? Description : Name);
        public string GetSeoKeywords(int lang = 1) => !string.IsNullOrWhiteSpace(MetaKeywords) ? MetaKeywords : Name;
    }
}
