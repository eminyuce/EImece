using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class AuthorStoriesViewModel : ItemListing
    {
        public string AuthorName { get; set; }
        public PaginatedList<Story> Stories { get; set; }

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