using Resources;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Models.AdminModels
{
    public class SystemSettingModel
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmailHost))]
        public string AdminEmailHost { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmailPassword))]
        public string AdminEmailPassword { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmailEnableSsl))]
        public bool AdminEmailEnableSsl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmailPort))]
        public int AdminEmailPort { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmailDisplayName))]
        public string AdminEmailDisplayName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmailUseDefaultCredentials))]
        public bool AdminEmailUseDefaultCredentials { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminUserName))]
        public string AdminUserName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.DefaultImageHeight))]
        public int DefaultImageHeight { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.DefaultImageWidth))]
        public int DefaultImageWidth { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.GoogleAnalyticsTrackingScript))]
        public string GoogleAnalyticsTrackingScript { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.WhatsAppCommunicationScript))]
        public string WhatsAppCommunicationScript { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.GoogleMapScript))]
        public string GoogleMapScript { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FacebookWebSiteLink))]
        public string FacebookWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LinkedinWebSiteLink))]
        public string LinkedinWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TwitterWebSiteLink))]
        public string TwitterWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.InstagramWebSiteLink))]
        public string InstagramWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PinterestWebSiteLink))]
        public string PinterestWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.YoutubeWebSiteLink))]
        public string YoutubeWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Zopim))]
        public string Zopim { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CompanyAddress))]
        public string CompanyAddress { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.WebSiteCompanyPhoneAndLocation))]
        public string WebSiteCompanyPhoneAndLocation { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.WebSiteCompanyEmailAddress))]
        [EmailAddress]
        public string WebSiteCompanyEmailAddress { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsProductPriceEnable))]
        public bool IsProductPriceEnable { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CompanyName))]
        public string CompanyName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.BasketMinTotalPrice))]
        public int BasketMinTotalPriceForCargo { get; set; }
    }
}