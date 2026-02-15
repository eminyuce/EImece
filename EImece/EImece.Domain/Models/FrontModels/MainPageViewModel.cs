using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class MainPageViewModel
    {
        public List<MainPageImage> MainPageImages { get; set; }
        public List<MainPageImageDto> MainPageImagesDto { get; set; }

        public List<ProductCategory> MainPageProductCategories { set; get; }
        public List<ProductCategoryDto> MainPageProductCategoriesDto { set; get; }

        public List<Product> MainPageProducts { get; set; }
        public List<ProductDto> MainPageProductsDto { get; set; }
        public List<Product> LatestProducts { get; set; }
        public List<ProductDto> LatestProductsDto { get; set; }
        public List<Product> CampaignProducts { get; set; }
        public List<ProductDto> CampaignProductsDto { get; set; }

        public Menu MainPageMenu { get; set; }
        public MenuDto MainPageMenuDto { get; set; }

        public List<Story> LatestStories { get; set; }
        public List<StoryDto> LatestStoriesDto { get; set; }
        public StoryIndexViewModel StoryIndexViewModel { get; set; }
        public int CurrentLanguage { get; set; }
    }
}
