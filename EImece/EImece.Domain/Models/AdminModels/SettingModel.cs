using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Domain.Models.AdminModels
{
    public class SettingModel
    {

        public string AdminEmailHost { get; set; }
        public string AdminEmailPassword { get; set; }
        public bool AdminEmailEnableSsl { get; set; }
        public int AdminEmailPort { get; set; }
        public string AdminEmailDisplayName { get; set; }
        public string AdminEmail { get; set; }
        public bool AdminEmailUseDefaultCredentials { get; set; }
        public string AdminUserName { get; set; }
        public int DefaultImageHeight { get; set; }
        public int DefaultImageWidth { get; set; }
        //public string SiteIndexMetaTitle { get; set; }
        //public string SiteIndexMetaDescription { get; set; }
        //public string SiteIndexMetaKeywords { get; set; }
        [AllowHtml]
        public string GoogleAnalyticsTrackingScript { get; set; }
        [AllowHtml]
        public string FacebookWebSiteLink { get; set; }
        [AllowHtml]
        public string GooglePlusWebSiteLink { get; set; }
        [AllowHtml]
        public string LinkedinWebSiteLink { get; set; }
        [AllowHtml]
        public string TwitterWebSiteLink { get; set; }
        [AllowHtml]
        public string InstagramWebSiteLink { get; set; }
        [AllowHtml]
        public string YoutubeWebSiteLink { get; set; }
        [AllowHtml]
        public string Zopim { get; set; }
        public string CompanyAddress { get; set; }
        public string WebSiteCompanyPhoneAndLocation { get; set; }
        public string WebSiteCompanyEmailAddress { get; set; }
        //public string PrivacyPolicy { get; set; }
        //public string TermsAndConditions { get; set; }
        public bool IsProductPriceEnable { get; set; }
        [Display(Name = "Site Languages")]
        public string Languages { get; set; }
        public string CompanyName { get; set; }
        public string SiteIndexMetaTitle { get; set; }
        public string SiteIndexMetaDescription { get; set; }
        public string SiteIndexMetaKeywords { get; set; }
        //public string FooterDescription { get;  set; }
        //public string FooterEmailListDescription { get;  set; }




    }
}
