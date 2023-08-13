using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class MainPageViewModel
    {
        public string DeliveryInfoExists { get; set; }

        public List<MainPageImage> MainPageImages { get; set; }

        public List<ProductCategory> MainPageProductCategories { set; get; }

        public List<Product> MainPageProducts { get; set; }
        public List<Product> LatestProducts { get; set; }
        public List<Product> CampaignProducts { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<Story> LatestStories { get; set; }
        public StoryIndexViewModel StoryIndexViewModel { get; set; }
        public int CurrentLanguage { get; set; }

        public List<Setting> ApplicationSettings { get; set; }

        private Setting GetSetting(string key)
        {
            if (ApplicationSettings == null)
            {
                return null;
            }
            return ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsProductPriceEnable
        {
            get
            {
                var item = GetSetting(Constants.IsProductPriceEnable);
                if (item == null)
                {
                    return false;
                }
                else
                {
                    return item.SettingValue.ToStr().Equals("true", StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
        public bool IsProductCommentSectionEnable
        {
            get
            {
                var item = GetSetting(Constants.IsProductCommentSectionEnable);
                if (item == null)
                {
                    return false;
                }
                else
                {
                    return item.SettingValue.ToStr().Equals("true", StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
    }
}