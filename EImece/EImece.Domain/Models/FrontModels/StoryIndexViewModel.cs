using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryIndexViewModel : ItemListing
    {
        public PaginatedList<Story> Stories { get; set; }
        public List<StoryCategory> StoryCategories { get; set; }
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