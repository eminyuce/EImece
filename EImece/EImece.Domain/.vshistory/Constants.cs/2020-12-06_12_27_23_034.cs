using System.Collections.Immutable;

namespace EImece.Domain
{
    public static class Constants
    {
        /*********ControllerRoutingPrefix*********/
        public const string ProductsCategoriesControllerRoutingPrefix = "c"; // categories
        public const string PagesControllerRoutingPrefix = "i";  // info pages
        public const string StoriesCategoriesControllerRoutingPrefix = "s";  // stories
        public const string ProductsControllerRoutingPrefix = "p";  //products
        /*********ActionRoutingPrefix*********/
        public const string SearchProductPrefix = "arama";
        public const string ProductTagPrefix = "t/{id}"; // tags
        public const string StoryTagPrefix = "t/{id}"; // tags
        public const string CategoryPrefix = "pc/{id}";
       

        public const string INFO_PREFIX = "info-";
        public const string IyzicoDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string OrderGuidCookieKey = "orderGuid";
        public const string SUCCESS = "SUCCESS";
        public const string FAILED = "FAILED";
        public const string EN_US_CULTURE_INFO = "en-US";
        /*********CACHE KEYS*********/
        public const string Cache30Days = "Cache30Days";
        public const string Cache20Minutes = "Cache20Minutes";
        public const string ImageProxyCaching = "ImageProxyCaching";
        public const string Cache1Hour = "Cache1Hour";
        /********* SETTING KEYS **********/
        public const string AdminEmailHost = "AdminEmailHost";
        public const string AdminEmailPassword = "AdminEmailPassword";
        public const string AdminEmailEnableSsl = "AdminEmailEnableSsl";
        public const string AdminEmailPort = "AdminEmailPort";
        public const string AdminEmailDisplayName = "AdminEmailDisplayName";
        public const string AdminEmail = "AdminEmail";
        public const string AdminEmailUseDefaultCredentials = "AdminEmailUseDefaultCredentials";
        public const string AdminUserName = "AdminUserName";
        public const string WebSiteCompanyEmailAddress = "WebSiteCompanyEmailAddress";
        public const string DefaultImageHeight = "DefaultImageHeight";
        public const string DefaultImageWidth = "DefaultImageWidth";
     
        public const string SiteIndexMetaTitle = "SiteIndexMetaTitle";
        public const string IsProductPriceEnable = "IsProductPriceEnable";
        public const string GoogleMapScript = "GoogleMapScript";
        public const string SiteIndexMetaDescription = "SiteIndexMetaDescription";
        public const string SiteIndexMetaKeywords = "SiteIndexMetaKeywords";
        public const string SpecialPage = "Special_Page";
        public const string AdminSetting = "AdminSetting";
        public const string SystemSettings = "SystemSettings";
        public const string TermsAndConditions = "TermsAndConditions";
        public const string AboutUs = "AboutUs";
        public const string DeliveryInfo = "DeliveryInfo";
        public const string WebSiteLogo = "WebSiteLogo";
        public const string CompanyName = "CompanyName";  
        public const string CompanyAddress = "CompanyAddress";
        public const string OrderConfirmationEmailMailTemplate = "OrderConfirmationEmail";
        public const string ConfirmYourAccountMailTemplate = "ConfirmYourAccount";
        public const string ForgotPasswordMailTemplate = "ForgotPassword";
        public const string ContactUsAboutProductInfoMailTemplate = "ContactUsAboutProductInfo";
        public const string ContactUsForCommunication = "ContactUsForCommunication";
        public const string SendMessageToSellerMailTemplate = "SendMessageToSeller";
        public const string WebSiteCompanyPhoneAndLocation = "WebSiteCompanyPhoneAndLocation";
        public const string InstagramWebSiteLink = "InstagramWebSiteLink";
        public const string PinterestWebSiteLink = "PinterestWebSiteLink";
        public const string TwitterWebSiteLink = "TwitterWebSiteLink";
        public const string LinkedinWebSiteLink = "LinkedinWebSiteLink";
        public const string FacebookWebSiteLink = "FacebookWebSiteLink";
        public const string YotubeWebSiteLink = "YotubeWebSiteLink";
        public const string GoogleAnalyticsTrackingScript = "GoogleAnalyticsTrackingScript";       
        public const string WhatsAppCommunicationScript = "WhatsAppCommunicationScript";
        public const string LastVisit = "LastVisit";
        public const string PrivacyPolicy = "PrivacyPolicy";
        public const string ELanguage = "ELanguage";

        public const string TempDataReturnUrlReferrer = "TempDataReturnUrlReferrer";

        public const string DbConnectionKey = "EImeceDbConnection";
        public const string AdministratorRole = "Admin";
        public const string EditorRole = "NormalUser";
        public const string CustomerRole = "Customer";
        public const string ImageActionName = "Index";
        public const int PartialViewOutputCachingDuration = 86400;
        public const string SelectedLanguage = "SelectedLanguage";

        public const string CargoCompany = "CargoCompany";
        public const string BasketMinTotalPriceForCargo = "BasketMinTotalPriceForCargo";
        public const string CargoPrice = "CargoPrice";  
        public const string CargoDescription = "CargoDescription";

        public const string TempPath = "~/media/tempFiles/";
        public const string ServerMapPath = "~/media/images/";
        public const string UrlBase = "/media/images/";
        public const string DeleteURL = "/Media/DeleteFile/?file={0}&contentId={1}&mod={2}&imageType={3}";
        public const string DeleteType = "GET";
        public const string FileUploadDeleteURL = "/FileUpload/DeleteFile/?file=";
        public const string AdminCultureCookieName = "_adminCulture";
        public const string CultureCookieName = "_culture";
        public const string ShoppingCartKey = "ShoppingCartKey1";
        public static ImmutableArray<string> NumbersArr = ImmutableArray.Create(new string[] { "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" });
        public const string PageThemeCommunication = "T8";
        public const string PageThemeImageGallery = "T7";
        public const string PageThemeT6 = "T6";
        public const string PageThemeT5 = "T5";
        public const string PageThemeT4 = "T4";
        public const string PageThemeT3 = "T3";
        public const string PageThemeT2 = "T2";
        public const string PageThemeT1 = "T1";

        public static object DeleteButtonHtmlAttribute
        {
            get
            {
                return new { @class = "btn btn-sm btn-danger   glyphicon glyphicon-trash glyphicon-white" };
            }
        }

        public static string OkStyle
        {
            get
            {
                return "class='gridActiveIcon glyphicon glyphicon-ok-circle'";
            }
        }

        public static string CancelStyle
        {
            get
            {
                return "class='gridNotActiveIcon glyphicon  glyphicon-remove-circle'";
            }
        }
    }
}