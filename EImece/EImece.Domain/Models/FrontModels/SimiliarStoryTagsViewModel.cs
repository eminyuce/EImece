using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;

namespace EImece.Domain.Models.FrontModels
{
    public class SimiliarStoryTagsViewModel
    {
        public StorefrontTagDto Tag { get; set; }
        public PaginatedList<StorefrontStoryCardDto> StoryTags { get; set; }

        public PaginatedList<StorefrontProductCardDto> ProductTags { get; set; }

        public SettingValueDto CompanyName { get; set; }
    }
}