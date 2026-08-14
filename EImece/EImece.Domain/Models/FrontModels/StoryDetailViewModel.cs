using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryDetailViewModel
    {
        public Story Story { get; set; }

        public List<StoryCategory> StoryCategories { get; set; }

        public List<Story> RelatedStories { get; set; }

        public List<Story> FeaturedStories { get; set; }

        public List<StorefrontProductCardDto> RelatedProducts { get; set; }

        public Menu BlogMenu { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<Tag> Tags { get; set; }

        public Story PreviousStory  { get; set; }
        public Story NextStory { get; set; }

        public Dictionary<string, string> SocialMediaLinks { get; set; }

        public StorefrontStoryDetailDto StorefrontStory { get; set; }
        public List<StorefrontStoryCardDto> StorefrontRelatedStories { get; set; }
        public List<StorefrontStoryCardDto> StorefrontFeaturedStories { get; set; }
        public StorefrontStoryCardDto StorefrontPreviousStory { get; set; }
        public StorefrontStoryCardDto StorefrontNextStory { get; set; }
    }
}