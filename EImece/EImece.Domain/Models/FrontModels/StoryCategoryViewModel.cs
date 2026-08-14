using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryCategoryViewModel
    {
        public StorefrontCategoryDto StoryCategory { get; set; }

        public StorefrontMenuDto MainPageMenu { get; set; }

        public List<StorefrontCategoryDto> StoryCategories { get; set; }
        public List<StorefrontTagDto> Tags { get; set; }

        public PaginatedList<StorefrontStoryCardDto> StorefrontStories { get; set; }

        public StoryCategoryViewModel()
        {
            StoryCategories = new List<StorefrontCategoryDto>();
            Tags = new List<StorefrontTagDto>();
        }
    }
}