using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryDetailViewModel
    {
        public Story Story { get; set; }

        public List<StoryCategory> StoryCategories { get; set; }

        public List<Story> RelatedStories { get; set; }

        public List<Story> FeaturedStories { get; set; }

        public List<Product> RelatedProducts { get; set; }

        public Menu BlogMenu { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<Tag> Tags { get; set; }
    }
}