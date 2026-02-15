using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryIndexViewModel
    {
        public PaginatedList<Story> Stories { get; set; }
        public PaginatedList<StoryDto> StoriesDto { get; set; }
        public List<StoryCategory> StoryCategories { get; set; }
        public List<StoryCategoryDto> StoryCategoriesDto { get; set; }
    }
}