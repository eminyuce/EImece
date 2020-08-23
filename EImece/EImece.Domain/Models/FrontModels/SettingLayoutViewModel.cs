using EImece.Domain.Entities;

namespace EImece.Domain.Models.FrontModels
{
    public class SettingLayoutViewModel
    {
        public Setting WebSiteCompanyPhoneAndLocation { get; set; }
        public Setting WebSiteCompanyEmailAddress { get; set; }
        public Setting WebSiteLogo { get; set; }
        public Setting CompanyName { get; set; }

        public bool isMobilePage { get; set; }
    }
}