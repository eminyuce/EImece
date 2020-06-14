using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class MainPageViewModel
    {
        public List<MainPageImage> MainPageImages { get; set; }

        public List<ProductCategory> MainPageProductCategories { set; get; }

        public List<Product> MainPageProducts { get; set; }
        public List<Product> LatestProducts { get; set; }
        public List<Product> CampaignProducts { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<Story> LatestStories { get; set; }
        public StoryIndexViewModel StoryIndexViewModel { get; set; }
        public int CurrentLanguage { get; set; }
    }
}