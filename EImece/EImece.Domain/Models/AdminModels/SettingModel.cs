using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.AdminModels
{
    public class SettingModel
    {
        public string AdminEmailHost { get; set; }
        public string AdminEmailPassword { get; set; }
        public string AdminEmailEnableSsl { get; set; }
        public string AdminEmailPort { get; set; }
        public string AdminEmailDisplayName { get; set; }
        public string AdminEmail { get; set; }
        public string AdminEmailUseDefaultCredentials { get; set; }
        public string AdminUserName { get; set; }
        public string DefaultImageHeight { get; set; }
        public string DefaultImageWidth { get; set; }
        //public string SiteIndexMetaTitle { get; set; }
        //public string SiteIndexMetaDescription { get; set; }
        //public string SiteIndexMetaKeywords { get; set; }
        public string GoogleAnalyticsTrackingScript { get; set; }
        public string FacebookWebSiteLink { get; set; }
        public string GooglePlusWebSiteLink { get; set; }
        public string LinkedinWebSiteLink { get; set; }
        public string TwitterWebSiteLink { get; set; }
        public string InstagramWebSiteLink { get; set; }
        public string YoutubeWebSiteLink { get; set; }
        public string Zopim { get; set; }
        public string CompanyAddress { get; set; }
        public string WebSiteLogo { get; set; }
        public string WebSiteCompanyPhoneAndLocation { get; set; }
        public string WebSiteCompanyEmailAddress { get; set; }
        //public string PrivacyPolicy { get; set; }
        //public string TermsAndConditions { get; set; }
        //public string FooterEmailListDescription { get; set; }
        //public string FooterDescription { get; set; }
        public string IsProductPriceEnable { get; set; }
        //public string CompanyName { get; set; }



    }
}
