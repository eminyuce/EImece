using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class MainPageViewModel
    {
        public List<MainPageImageDto> MainPageImages { get; set; }

        public List<ProductCategoryDto> MainPageProductCategories { set; get; }

        public List<ProductDto> MainPageProducts { get; set; }
        public List<ProductDto> LatestProducts { get; set; }
        public List<ProductDto> CampaignProducts { get; set; }

        public MenuDto MainPageMenu { get; set; }

        public List<StoryDto> LatestStories { get; set; }
        public StoryIndexViewModel StoryIndexViewModel { get; set; }
        public int CurrentLanguage { get; set; }
    }
}