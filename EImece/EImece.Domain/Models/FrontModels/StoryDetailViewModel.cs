using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<Setting> ApplicationSettings { get; set; }

        private Setting GetSetting(string key)
        {
            if (ApplicationSettings == null)
            {
                return null;
            }
            return ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }


    }
}