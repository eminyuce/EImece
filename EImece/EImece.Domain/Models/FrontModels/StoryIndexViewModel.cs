using EImece.Domain.Entities;
using GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryIndexViewModel
    {
        public PaginatedList<Story> Stories { get; set; }
        public List<StoryCategory> StoryCategories { get; set; }
    }
}