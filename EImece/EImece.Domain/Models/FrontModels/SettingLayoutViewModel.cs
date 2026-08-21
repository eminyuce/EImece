using EImece.Domain.Models.DTOs;

namespace EImece.Domain.Models.FrontModels
{
    public class SettingLayoutViewModel
    {
        public SettingDto WebSiteCompanyPhoneAndLocation { get; set; }
        public SettingDto WebSiteCompanyEmailAddress { get; set; }
        public SettingDto WebSiteLogo { get; set; }
        public SettingDto CompanyName { get; set; }

        public bool isMobilePage { get; set; }
    }
}