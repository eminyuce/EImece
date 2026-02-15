using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryDetailViewModel
    {
        public Story Story { get; set; }

        public StoryDto StoryDto { get; set; }

        public List<StoryCategory> StoryCategories { get; set; }

        public List<StoryCategoryDto> StoryCategoriesDto { get; set; }

        public List<Story> RelatedStories { get; set; }

        public List<StoryDto> RelatedStoriesDto { get; set; }

        public List<Story> FeaturedStories { get; set; }

        public List<StoryDto> FeaturedStoriesDto { get; set; }

        public List<Product> RelatedProducts { get; set; }

        public List<ProductDto> RelatedProductsDto { get; set; }

        public Menu BlogMenu { get; set; }

        public MenuDto BlogMenuDto { get; set; }

        public Menu MainPageMenu { get; set; }

        public MenuDto MainPageMenuDto { get; set; }

        public List<Tag> Tags { get; set; }

        public List<TagDto> TagsDto { get; set; }

        public Story PreviousStory  { get; set; }
        public StoryDto PreviousStoryDto { get; set; }
        public Story NextStory { get; set; }
        public StoryDto NextStoryDto { get; set; }
    }
}