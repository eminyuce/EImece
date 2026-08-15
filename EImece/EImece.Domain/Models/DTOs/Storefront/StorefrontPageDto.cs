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
        public DateTime UpdatedDate { get; set; }

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

        public string GetCroppedImageUrl(int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = MainImageId.HasValue ? MainImageId.Value : 0;
            var dummy = new Menu { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }
    }
}
