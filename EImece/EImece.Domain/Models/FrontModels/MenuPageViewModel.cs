using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class MenuPageViewModel
    {
        public Menu Menu { get; set; }

        public Setting CompanyName
        {
            get
            {
                return GetSetting(Constants.CompanyName);
            }
        }

        private Setting GetSetting(string key)
        {
            return ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        private String GetSettingValue(string key)
        {
            var item = ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                return item.SettingValue;
            }
            else
            {
                return String.Empty;
            }
        }

        public Menu MainPageMenu { get; set; }

        public ContactUsFormViewModel Contact { get; set; }
        public List<Setting> ApplicationSettings { get; set; }

        public Setting GoogleMapScript
        {
            get
            {
                var result = ApplicationSettings.FirstOrDefault(r => r.SettingKey.Equals(Constants.GoogleMapScript, StringComparison.InvariantCultureIgnoreCase));
                if (result == null)
                {
                    result = new Setting();
                }
                return result;
            }
        }
    }
}