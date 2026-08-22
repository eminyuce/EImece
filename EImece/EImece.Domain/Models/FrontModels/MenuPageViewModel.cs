using EImece.Domain.Models.DTOs.Storefront;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class MenuPageViewModel
    {
        public StorefrontMenuDto Menu { get; set; }

        public SettingKeyValueDto CompanyName
        {
            get
            {
                return GetSetting(Constants.CompanyName);
            }
        }

        public SettingKeyValueDto CompanyAddress
        {
            get
            {
                return GetSetting(Constants.CompanyAddress);
            }
        }

        public SettingKeyValueDto WebSiteCompanyPhoneAndLocation
        {
            get
            {
                return GetSetting(Constants.WebSiteCompanyPhoneAndLocation);
            }
        }

        public SettingKeyValueDto WebSiteCompanyEmailAddress
        {
            get
            {
                return GetSetting(Constants.WebSiteCompanyEmailAddress);
            }
        }

        private SettingKeyValueDto GetSetting(string key)
        {
            if (ApplicationSettings == null) return new SettingKeyValueDto { SettingKey = key, SettingValue = string.Empty };
            return ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase)) ?? new SettingKeyValueDto { SettingKey = key, SettingValue = string.Empty };
        }


        /// <summary>
        /// Sibling (or child) pages used by themes T5/T6 left navigation.
        /// </summary>
        public List<StorefrontMenuDto> SideMenus { get; set; }

        public ContactUsFormViewModel Contact { get; set; }
        public List<SettingKeyValueDto> ApplicationSettings { get; set; }

        public Dictionary<string, string> SocialMediaLinks { get; set; }

        public SettingKeyValueDto GoogleMapScript
        {
            get
            {
                var result = GetSetting(Constants.GoogleMapScript);
                if (result == null)
                {
                    result = new SettingKeyValueDto { SettingKey = Constants.GoogleMapScript, SettingValue = string.Empty };
                }
                return result;
            }
        }

        public MenuPageViewModel()
        {
            SideMenus = new List<StorefrontMenuDto>();
            ApplicationSettings = new List<SettingKeyValueDto>();
            SocialMediaLinks = new Dictionary<string, string>();
        }
    }
}