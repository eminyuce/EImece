using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryIndexViewModel
    {
        public List<StorefrontCategoryDto> StoryCategories { get; set; }

        public PaginatedList<StorefrontStoryCardDto> StorefrontStories { get; set; }

        public StoryIndexViewModel()
        {
            StoryCategories = new List<StorefrontCategoryDto>();
        }
    }
}