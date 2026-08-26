using Resources;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Models.AdminModels
{
    public class SystemSettingModel
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminEmail))]
        [EmailAddress]
        public string AdminEmail { get; set; }

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
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.WhatsAppCommunicationLink))]
        public string WhatsAppCommunicationLink { get; set; }

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

        // Property name is the SettingKey. Keep the historical typo "YotubeWebSiteLink"
        // so admin save/load matches Constants / seed / storefront HomeController.
        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.YoutubeWebSiteLink))]
        public string YotubeWebSiteLink { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Zopim))]
        public string Zopim { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsProductPriceEnable))]
        public bool IsProductPriceEnable { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PaymentDetailHtml))]
        public string PaymentDetailHtml { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsProductReviewEnable))]
        public bool IsProductReviewEnable { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductPriceFilterSetting))]
        public string ProductPriceFilterSetting { get; set; }

        // ========== 1. Site Maintenance & SEO ==========
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsSiteUnderConstruction))]
        public bool IsSiteUnderConstruction { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.UnderConstructionHtml))]
        public string UnderConstructionHtml { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AllowSearchEngineIndexing))]
        public bool AllowSearchEngineIndexing { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ActiveDesign))]
        public string ActiveDesign { get; set; }

        // ========== 2. PWA & Web App Manifest Branding ==========
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ThemeColor))]
        [RegularExpression(@"^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.HexColorErrorMessage))]
        public string ThemeColor { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ManifestBackgroundColor))]
        [RegularExpression(@"^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.HexColorErrorMessage))]
        public string ManifestBackgroundColor { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ManifestDisplay))]
        public string ManifestDisplay { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ManifestOrientation))]
        public string ManifestOrientation { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ManifestStartUrl))]
        public string ManifestStartUrl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ManifestFallbackName))]
        public string ManifestFallbackName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ManifestShortNameMaxLength))]
        [Range(1, 100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To100ErrorMessage))]
        public int ManifestShortNameMaxLength { get; set; }

        // ========== 3. Admin & Content UI Preferences ==========
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AdminPanelLanguage))]
        public string AdminPanelLanguage { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.GridPageSizeNumber))]
        [Range(1, 1000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1000ErrorMessage))]
        public int GridPageSizeNumber { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductShortDescriptionPreviewLength))]
        [Range(10, 5000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range10To5000ErrorMessage))]
        public int ProductShortDescriptionPreviewLength { get; set; }

        // ========== 4. Media & Image Upload Policies ==========
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadMaxWidth))]
        [Range(0, 10000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range0To10000ErrorMessage))]
        public int ImageUploadMaxWidth { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadMaxHeight))]
        [Range(0, 10000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range0To10000ErrorMessage))]
        public int ImageUploadMaxHeight { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadJpegQuality))]
        [Range(40, 95, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range40To95ErrorMessage))]
        public int ImageUploadJpegQuality { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadPreferWebP))]
        public bool ImageUploadPreferWebP { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadWebPQuality))]
        [Range(40, 100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range40To100ErrorMessage))]
        public int ImageUploadWebPQuality { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadSaveWebPSidecar))]
        public bool ImageUploadSaveWebPSidecar { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadThumbMaxWidth))]
        [Range(0, 5000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range0To5000ErrorMessage))]
        public int ImageUploadThumbMaxWidth { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadThumbMaxHeight))]
        [Range(0, 5000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range0To5000ErrorMessage))]
        public int ImageUploadThumbMaxHeight { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadThumbJpegQuality))]
        [Range(40, 95, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range40To95ErrorMessage))]
        public int ImageUploadThumbJpegQuality { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ImageUploadKeepOriginalIfSmaller))]
        public bool ImageUploadKeepOriginalIfSmaller { get; set; }

        // ========== 5. Payments & E-Commerce Options ==========
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PaymentProvider))]
        public string PaymentProvider { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IyzicoEnabledInstallments))]
        public string IyzicoEnabledInstallments { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.BuyerIdentityNumber))]
        public string BuyerIdentityNumber { get; set; }

        // ========== 6. Captcha & Anti-Spam / Rate Limiting ==========
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.CaptchaProvider))]
        public string CaptchaProvider { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RecaptchaSiteKey))]
        public string RecaptchaSiteKey { get; set; }

        // ========== 6b. Security / Two-Factor Authentication ==========
        /// <summary>
        /// Master switch for Authenticator (TOTP) two-factor authentication across the app.
        /// Stored as the "RequireAdminAuthenticator" Setting row, so AppConfig.RequireAdminAuthenticator
        /// resolves it through the SettingResolver before falling back to Web.config.
        /// When false: admins are not forced to enrol and enrolled admins are not challenged at login.
        /// </summary>
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RequireAdminAuthenticator))]
        public bool RequireAdminAuthenticator { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Enabled))]
        public bool RateLimit_Enabled { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Login_Limit))]
        [Range(1, 1000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1000ErrorMessage))]
        public int RateLimit_Login_Limit { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Login_WindowMinutes))]
        [Range(1, 1440, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1440ErrorMessage))]
        public int RateLimit_Login_WindowMinutes { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Contact_Limit))]
        [Range(1, 1000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1000ErrorMessage))]
        public int RateLimit_Contact_Limit { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Contact_WindowMinutes))]
        [Range(1, 1440, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1440ErrorMessage))]
        public int RateLimit_Contact_WindowMinutes { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Checkout_Limit))]
        [Range(1, 1000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1000ErrorMessage))]
        public int RateLimit_Checkout_Limit { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Checkout_WindowMinutes))]
        [Range(1, 1440, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1440ErrorMessage))]
        public int RateLimit_Checkout_WindowMinutes { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Search_Limit))]
        [Range(1, 1000, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1000ErrorMessage))]
        public int RateLimit_Search_Limit { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RateLimit_Search_WindowMinutes))]
        [Range(1, 1440, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.Range1To1440ErrorMessage))]
        public int RateLimit_Search_WindowMinutes { get; set; }
    }
}