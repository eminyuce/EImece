using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class MenuPageViewModel
    {
        public StorefrontMenuDto Menu { get; set; }

        public SettingDto CompanyName
        {
            get
            {
                return GetSetting(Constants.CompanyName);
            }
        }

        public SettingDto CompanyAddress
        {
            get
            {
                return GetSetting(Constants.CompanyAddress);
            }
        }

        public SettingDto WebSiteCompanyPhoneAndLocation
        {
            get
            {
                return GetSetting(Constants.WebSiteCompanyPhoneAndLocation);
            }
        }

        public SettingDto WebSiteCompanyEmailAddress
        {
            get
            {
                return GetSetting(Constants.WebSiteCompanyEmailAddress);
            }
        }

        private SettingDto GetSetting(string key)
        {
            if (ApplicationSettings == null) return new SettingDto();
            return ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase)) ?? new SettingDto();
        }

        public StorefrontMenuDto MainPageMenu { get; set; }

        /// <summary>
        /// Sibling (or child) pages used by themes T5/T6 left navigation.
        /// </summary>
        public List<StorefrontMenuDto> SideMenus { get; set; }

        public ContactUsFormViewModel Contact { get; set; }
        public List<SettingDto> ApplicationSettings { get; set; }

        public Dictionary<string, string> SocialMediaLinks { get; set; }

        public SettingDto GoogleMapScript
        {
            get
            {
                var result = GetSetting(Constants.GoogleMapScript);
                if (result == null)
                {
                    result = new SettingDto();
                }
                return result;
            }
        }

        public MenuPageViewModel()
        {
            SideMenus = new List<StorefrontMenuDto>();
            ApplicationSettings = new List<SettingDto>();
            SocialMediaLinks = new Dictionary<string, string>();
        }
    }
}