using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected banner / slider read model for storefront home page.
    /// </summary>
    public class StorefrontBannerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string Url { get; set; }
        public int? MainImageId { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }

        public string GetCroppedImageUrl(int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = MainImageId.HasValue ? MainImageId.Value : 0;
            var dummy = new MainPageImage { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetResponsiveImageSrcSet(int width = 0, int height = 0)
        {
            if (!MainImageId.HasValue) return string.Empty;
            var dummy = new MainPageImage { Id = Id, Name = Name };
            return dummy.GetResponsiveImageSrcSet(MainImageId.Value, width, height);
        }
    }
}
