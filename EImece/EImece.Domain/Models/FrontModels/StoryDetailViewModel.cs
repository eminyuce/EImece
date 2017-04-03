using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryDetailViewModel
    {
        public Story Story { get; set; }

        public List<Story> RelatedStories { get; set; }

        public List<Product> RelatedProducts { get; set; }

        public Menu BlogMenu { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<Tag> Tags { get; set; }
    }
}
