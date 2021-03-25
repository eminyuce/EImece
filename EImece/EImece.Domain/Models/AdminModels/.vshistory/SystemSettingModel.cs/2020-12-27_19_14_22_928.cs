using Resources;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Models.AdminModels
{
    public class SystemSettingModel
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminEmailHost))]
        public string AdminEmailHost { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminEmailPassword))]
        public string AdminEmailPassword { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminEmailEnableSsl))]
        public bool AdminEmailEnableSsl { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminEmailPort))]
        public int AdminEmailPort { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminEmailDisplayName))]
        public string AdminEmailDisplayName { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminEmailUseDefaultCredentials))]
        public bool AdminEmailUseDefaultCredentials { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AdminUserName))]
        public string AdminUserName { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.DefaultImageHeight))]
        public int DefaultImageHeight { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.DefaultImageWidth))]
        public int DefaultImageWidth { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.GoogleAnalyticsTrackingScript))]
        public string GoogleAnalyticsTrackingScript { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.WhatsAppCommunicationScript))]
        public string WhatsAppCommunicationScript { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.GoogleMapScript))]
        public string GoogleMapScript { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FacebookWebSiteLink))]
        public string FacebookWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LinkedinWebSiteLink))]
        public string LinkedinWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TwitterWebSiteLink))]
        public string TwitterWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.InstagramWebSiteLink))]
        public string InstagramWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PinterestWebSiteLink))]
        public string PinterestWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.YoutubeWebSiteLink))]
        public string YoutubeWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Zopim))]
        public string Zopim { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CompanyAddress))]
        public string CompanyAddress { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.WebSiteCompanyPhoneAndLocation))]
        public string WebSiteCompanyPhoneAndLocation { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.WebSiteCompanyEmailAddress))]
        [EmailAddress]
        public string WebSiteCompanyEmailAddress { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.IsProductPriceEnable))]
        public bool IsProductPriceEnable { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CompanyName))]
        public string CompanyName { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.BasketMinTotalPrice))]
        public int BasketMinTotalPriceForCargo { get; set; }
    }
}