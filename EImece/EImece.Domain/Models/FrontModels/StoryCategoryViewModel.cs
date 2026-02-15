using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryCategoryViewModel
    {
        public StoryCategory StoryCategory { get; set; }
        public StoryCategoryDto StoryCategoryDto { get; set; }

        public Menu MainPageMenu { get; set; }
        public MenuDto MainPageMenuDto { get; set; }

        public List<StoryCategory> StoryCategories { get; set; }
        public List<StoryCategoryDto> StoryCategoriesDto { get; set; }
        public List<Tag> Tags { get; set; }
        public List<TagDto> TagsDto { get; set; }
        public PaginatedList<Story> Stories { get; set; }
        public PaginatedList<StoryDto> StoriesDto { get; set; }
    }
}