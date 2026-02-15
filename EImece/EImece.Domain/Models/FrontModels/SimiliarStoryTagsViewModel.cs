using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.GenericRepository;

namespace EImece.Domain.Models.FrontModels
{
    public class SimiliarStoryTagsViewModel
    {
        public Tag Tag { get; set; }
        public TagDto TagDto { get; set; }
        public PaginatedList<StoryTag> StoryTags { get; set; }
        public PaginatedList<StoryTagDto> StoryTagsDto { get; set; }

        public PaginatedList<ProductTag> ProductTags { get; set; }

        public PaginatedList<ProductTagDto> ProductTagsDto { get; set; }

        public Setting CompanyName { get; set; }

        public SettingDto CompanyNameDto { get; set; }
    }
}