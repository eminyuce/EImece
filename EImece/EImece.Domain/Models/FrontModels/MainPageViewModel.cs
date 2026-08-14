using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class MainPageViewModel
    {
        public List<StorefrontBannerDto> MainPageImages { get; set; }

        public List<StorefrontCategoryDto> MainPageProductCategories { set; get; }

        public List<StorefrontProductCardDto> MainPageProducts { get; set; }
        public List<StorefrontProductCardDto> LatestProducts { get; set; }
        public List<StorefrontProductCardDto> CampaignProducts { get; set; }

        public StorefrontMenuDto MainPageMenu { get; set; }

        public List<StorefrontStoryCardDto> LatestStories { get; set; }
        public int CurrentLanguage { get; set; }
    }
}