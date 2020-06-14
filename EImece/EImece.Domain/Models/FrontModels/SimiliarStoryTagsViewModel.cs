using EImece.Domain.Entities;
using GenericRepository;

namespace EImece.Domain.Models.FrontModels
{
    public class SimiliarStoryTagsViewModel
    {
        public Tag Tag { get; set; }
        public PaginatedList<StoryTag> StoryTags { get; set; }

        public PaginatedList<ProductTag> ProductTags { get; set; }

        public Setting CompanyName { get; set; }
    }
}