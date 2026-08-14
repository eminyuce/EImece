using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryCategoryViewModel
    {
        public StoryCategory StoryCategory { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<StoryCategory> StoryCategories { get; set; }
        public List<Tag> Tags { get; set; }
        public PaginatedList<Story> Stories { get; set; }

        public PaginatedList<StorefrontStoryCardDto> StorefrontStories { get; set; }
    }
}