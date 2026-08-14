using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryDetailViewModel
    {
        public StorefrontStoryDetailDto StorefrontStory { get; set; }

        public List<StorefrontCategoryDto> StoryCategories { get; set; }

        public List<StorefrontStoryCardDto> StorefrontRelatedStories { get; set; }

        public List<StorefrontStoryCardDto> StorefrontFeaturedStories { get; set; }

        public List<StorefrontProductCardDto> RelatedProducts { get; set; }

        public StorefrontMenuDto BlogMenu { get; set; }

        public StorefrontMenuDto MainPageMenu { get; set; }

        public List<StorefrontTagDto> Tags { get; set; }

        public StorefrontStoryCardDto StorefrontPreviousStory { get; set; }
        public StorefrontStoryCardDto StorefrontNextStory { get; set; }

        public Dictionary<string, string> SocialMediaLinks { get; set; }

        public StoryDetailViewModel()
        {
            StoryCategories = new List<StorefrontCategoryDto>();
            StorefrontRelatedStories = new List<StorefrontStoryCardDto>();
            StorefrontFeaturedStories = new List<StorefrontStoryCardDto>();
            RelatedProducts = new List<StorefrontProductCardDto>();
            Tags = new List<StorefrontTagDto>();
            SocialMediaLinks = new Dictionary<string, string>();
        }
    }
}