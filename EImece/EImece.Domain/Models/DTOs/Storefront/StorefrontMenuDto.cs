using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected menu read model for navigation, header, and footer.
    /// </summary>
    public class StorefrontMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string MenuLink { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int? MainImageId { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public int TreeLevel { get; set; }
        public string PageTheme { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public bool ImageState
        {
            get { return MainImageId.HasValue && MainImageId.Value > 0; }
        }

        public List<StorefrontMenuDto> Children { get; set; }
        public List<StorefrontMenuDto> SideMenus { get; set; }
        public List<StorefrontMenuFileDto> MenuFiles { get; set; }

        public StorefrontMenuDto()
        {
            Children = new List<StorefrontMenuDto>();
            SideMenus = new List<StorefrontMenuDto>();
            MenuFiles = new List<StorefrontMenuFileDto>();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
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

        public string GetSeoTitle(int lang = 1)
        {
            return Name;
        }

        public string GetSeoDescription(int lang = 1)
        {
            return !string.IsNullOrWhiteSpace(ShortDescription) ? ShortDescription : (!string.IsNullOrWhiteSpace(Description) ? Description : Name);
        }

        public string GetSeoKeywords(int lang = 1)
        {
            return Name;
        }

        public string DetailPageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(Url)) return Url;
                var dummy = new Menu { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Pages");
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

        public string DetailPageLink
        {
            get { return DetailPageUrl; }
        }

        public string GetCroppedImageUrl(int? fileStorageId = null, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = fileStorageId.HasValue ? fileStorageId.Value : (MainImageId.HasValue ? MainImageId.Value : 0);
            var dummy = new Menu { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetCroppedImageTag(int width = 0, int height = 0)
        {
            return string.Format("<img src=\"{0}\" alt=\"{1}\" />", GetCroppedImageUrl(null, width, height), System.Web.HttpUtility.HtmlAttributeEncode(Name));
        }
    }
}
