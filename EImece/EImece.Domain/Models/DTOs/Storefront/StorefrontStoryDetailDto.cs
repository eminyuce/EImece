using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Purpose-specific read model for story detail pages.
    /// Includes full HTML content, active gallery files, active tags.
    /// </summary>
    public class StorefrontStoryDetailDto : StorefrontStoryCardDto
    {
        public string Description { get; set; }
        public string MetaKeywords { get; set; }

        public List<StorefrontProductFileDto> StoryFiles { get; set; }
        public List<StorefrontTagDto> StoryTags { get; set; }

        public StorefrontStoryDetailDto()
        {
            StoryFiles = new List<StorefrontProductFileDto>();
            StoryTags = new List<StorefrontTagDto>();
        }
    }
}
