using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected story card read model for homepage, story listing, and story category lists.
    /// </summary>
    public class StorefrontStoryCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public int StoryCategoryId { get; set; }
        public string StoryCategoryName { get; set; }
        public int? MainImageId { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public bool MainPage { get; set; }
        public bool IsFeaturedStory { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int ReadCount { get; set; }

        public List<StorefrontTagDto> Tags { get; set; }

        public StorefrontStoryCardDto()
        {
            Tags = new List<StorefrontTagDto>();
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
                var dummy = new Story { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Stories", StoryCategoryName ?? "no_category");
            }
        }

        public string StoryCategoryDetailPageUrl
        {
            get
            {
                if (StoryCategoryId <= 0) return string.Empty;
                var dummy = new StoryCategory { Id = StoryCategoryId, Name = StoryCategoryName };
                return dummy.GetDetailPageUrl("categories", "stories");
            }
        }

        public string GetCroppedImageUrl(int? fileStorageId = null, int width = 0, int height = 0, bool isFullPath = false, bool isThumb = false)
        {
            int imageId = fileStorageId.HasValue ? fileStorageId.Value : (MainImageId.HasValue ? MainImageId.Value : 0);
            var dummy = new Story { Id = Id, Name = Name };
            return dummy.GetCroppedImageUrl(imageId, width, height, isFullPath, isThumb);
        }

        public string GetResponsiveImageSrcSet(int fileStorageId, int width = 0, int height = 0)
        {
            var dummy = new Story { Id = Id, Name = Name };
            return dummy.GetResponsiveImageSrcSet(fileStorageId, width, height);
        }

        public string GetSeoDescription()
        {
            return GeneralHelper.GetDescriptionWithBody(ShortDescription, 100);
        }
    }
}
