using EImece.Domain.Models.DTOs.Storefront;

namespace EImece.Domain.Models.FrontModels
{
    public class SettingLayoutViewModel
    {
        public SettingValueDto WebSiteCompanyPhoneAndLocation { get; set; }
        public SettingValueDto WebSiteCompanyEmailAddress { get; set; }

        public bool isMobilePage { get; set; }
    }
}