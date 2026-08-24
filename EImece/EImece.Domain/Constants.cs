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
        public const string PaymentControllerRoutingPrefix = "o";  //orders
        /*********ActionRoutingPrefix*********/
        public const string SearchProductPrefix = "arama";
        public const string ProductTagPrefix = "t/{id}"; // tags
        public const string StoryTagPrefix = "t/{id}"; // tags
        public const string CategoryPrefix = "pc/{id}"; // product categories → /c/pc/{id}
        public const string StoryCategoryPrefix = "sc/{id}"; // story categories → /s/sc/{id}

        public const string LogoImagePath = "/images/logo.jpg";
        public const string UrlPathSeparator = "/";
        public const string ShipmentTrackingCompanyLink = "https://geliver.io/takip";
        public const string INFO_PREFIX = "info-";
        public const string IyzicoDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string OrderGuidCookieKey = "orderGuid";
        public const string SUCCESS = "SUCCESS";
        public const string FAILED = "FAILED";
        public const string EN_US_CULTURE_INFO = "en-US";
        /*********CACHE KEYS*********/
        public const string Cache30Days = "Cache30Days";
        public const string Cache10Days = "Cache10Days";
        public const string Cache1Day = "Cache1Day";
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

        public const string FooterEmailListDescription = "FooterEmailListDescription";
        public const string FooterHtmlDescription = "FooterHtmlDescription";
        public const string FooterDescription = "FooterDescription";

        public const string SiteIndexMetaTitle = "SiteIndexMetaTitle";
        public const string IsProductPriceEnable = "IsProductPriceEnable";
        public const string IsProductReviewEnable = "IsProductReviewEnable";
        public const string WhatsAppCommunicationLink = "WhatsAppCommunicationLink";
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
        public const string ThemeColor = "ThemeColor";
        public const string WebAppManifestContentType = "application/manifest+json";
        public const string CompanyAddress = "CompanyAddress";
        public const string CompanyGotNewOrderEmailMailTemplate = "CompanyGotNewOrderEmail";
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
        public const string SharePageUrl = "SharePageUrl";
        public const string GoogleAnalyticsTrackingScript = "GoogleAnalyticsTrackingScript";
        public const string WhatsAppCommunicationScript = "WhatsAppCommunicationScript";
        public const string LastVisit = "LastVisit";
        public const string PrivacyPolicy = "PrivacyPolicy";
        public const string ELanguage = "ELanguage";
        public const string ProductPriceFilterSetting = "ProductPriceFilterSetting";

        // Site Maintenance & SEO Setting Keys
        public const string IsSiteUnderConstruction = "IsSiteUnderConstruction";
        public const string UnderConstructionHtml = "UnderConstructionHtml";
        public const string DefaultUnderConstructionHtml = "";
        public const string AllowSearchEngineIndexing = "AllowSearchEngineIndexing";
        public const string ActiveDesign = "ActiveDesign";

        // PWA & Web App Manifest Setting Keys
        public const string ManifestBackgroundColor = "ManifestBackgroundColor";
        public const string ManifestDisplay = "ManifestDisplay";
        public const string ManifestOrientation = "ManifestOrientation";
        public const string ManifestStartUrl = "ManifestStartUrl";
        public const string ManifestFallbackName = "ManifestFallbackName";
        public const string ManifestShortNameMaxLength = "ManifestShortNameMaxLength";

        // Admin & Content UI Preferences Setting Keys
        public const string AdminPanelLanguage = "AdminPanelLanguage";
        public const string GridPageSizeNumber = "GridPageSizeNumber";
        public const string ProductShortDescriptionPreviewLength = "ProductShortDescriptionPreviewLength";
        public const string IsEditLinkEnable = "IsEditLinkEnable";
        public const string AdminImageHeightPercantage = "AdminImageHeightPercantage";
        public const string AdminImageWidthPercantage = "AdminImageWidthPercantage";

        // Media & Image Upload Policies Setting Keys
        public const string ImageUploadMaxWidth = "ImageUploadMaxWidth";
        public const string ImageUploadMaxHeight = "ImageUploadMaxHeight";
        public const string ImageUploadJpegQuality = "ImageUploadJpegQuality";
        public const string ImageUploadPreferWebP = "ImageUploadPreferWebP";
        public const string ImageUploadWebPQuality = "ImageUploadWebPQuality";
        public const string ImageUploadSaveWebPSidecar = "ImageUploadSaveWebPSidecar";
        public const string ImageUploadThumbMaxWidth = "ImageUploadThumbMaxWidth";
        public const string ImageUploadThumbMaxHeight = "ImageUploadThumbMaxHeight";
        public const string ImageUploadThumbJpegQuality = "ImageUploadThumbJpegQuality";
        public const string ImageUploadKeepOriginalIfSmaller = "ImageUploadKeepOriginalIfSmaller";

        // Payments & E-Commerce Options Setting Keys
        public const string PaymentProvider = "PaymentProvider";
        public const string IyzicoEnabledInstallments = "IyzicoEnabledInstallments";
        public const string BuyerIdentityNumber = "BuyerIdentityNumber";

        // Captcha & Anti-Spam / Security Setting Keys
        public const string CaptchaProvider = "CaptchaProvider";
        public const string RecaptchaSiteKey = "RecaptchaSiteKey";
        public const string RequireAdminAuthenticator = "RequireAdminAuthenticator";
        public const string RateLimit_Enabled = "RateLimit:Enabled";
        public const string RateLimit_Login_Limit = "RateLimit:Login:Limit";
        public const string RateLimit_Login_WindowMinutes = "RateLimit:Login:WindowMinutes";
        public const string RateLimit_Contact_Limit = "RateLimit:Contact:Limit";
        public const string RateLimit_Contact_WindowMinutes = "RateLimit:Contact:WindowMinutes";
        public const string RateLimit_Checkout_Limit = "RateLimit:Checkout:Limit";
        public const string RateLimit_Checkout_WindowMinutes = "RateLimit:Checkout:WindowMinutes";
        public const string RateLimit_Search_Limit = "RateLimit:Search:Limit";
        public const string RateLimit_Search_WindowMinutes = "RateLimit:Search:WindowMinutes";

        // Default Fallback Values for Admin System Settings
        public const string DefaultActiveDesign = "Crizal";
        public const bool DefaultAllowSearchEngineIndexing = false;
        public const string DefaultThemeColor = "#067a36";
        public const string DefaultManifestBackgroundColor = "#ffffff";
        public const string DefaultManifestDisplay = "standalone";
        public const string DefaultManifestOrientation = "portrait";
        public const string DefaultManifestStartUrl = "/";
        public const string DefaultManifestFallbackName = "Web App";
        public const int DefaultManifestShortNameMaxLength = 12;
        public const int DefaultGridPageSizeNumber = 100;
        public const int DefaultProductShortDescriptionPreviewLength = 180;
        public const int DefaultImageUploadMaxWidth = 1920;
        public const int DefaultImageUploadMaxHeight = 1920;
        public const int DefaultImageUploadJpegQuality = 82;
        public const int DefaultImageUploadWebPQuality = 82;
        public const int DefaultImageUploadThumbMaxWidth = 800;
        public const int DefaultImageUploadThumbMaxHeight = 800;
        public const int DefaultImageUploadThumbJpegQuality = 75;
        public const bool DefaultImageUploadPreferWebP = false;
        public const bool DefaultImageUploadSaveWebPSidecar = false;
        public const bool DefaultImageUploadKeepOriginalIfSmaller = true;
        public const string DefaultPaymentProvider = "Iyzico";
        public const string DefaultIyzicoEnabledInstallments = "1,2,4,6,9";
        public const string DefaultBuyerIdentityNumber = "11111111111";
        public const bool DefaultRequireAdminAuthenticator = true;
        public const string DefaultCaptchaProvider = "Legacy";
        public const string DefaultAdminPanelLanguage = "tr-TR";

        public const string DbConnectionKey = "EImeceDbConnection";
        public const string DbConnectionEnvironmentVariable = "EIMECE_DB_CONNECTION_STRING";
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
        public const string PaymentDetailHtml = "PaymentDetailHtml";

        public const string TempPath = "~/media/tempFiles/";
        public const string ServerMapPath = "~/media/images/";
        public const string UrlBase = "/media/images/";
        public const string DeleteURL = "/Media/DeleteFile/?file={0}&contentId={1}&mod={2}&imageType={3}";
        public const string DeleteType = "POST";
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
        public const string DefaultImageText = "X";

        public const string BuyNowCustomerUserId = "BNC";
        public const string ShoppingWithoutAccountUserId = "SWA";
        public const string LanguageSession = "LanguageSession";

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

        public static string CURRENCY_TURKISH = "TRY";
        public const string IYZICO_ADDRESS_COUNTRY = "Turkiye";
        public static string TR = "tr-TR";

        public const string IndexAction = "Index";
        public const string AdminAreaName = "admin";
        public const string DashboardAction = "Dashboard";
        public const string LockoutAction = "Lockout";
        public const string AdminLoginAction = "AdminLogin";
        public const string PaymentAction = "Payment";
        public const string DetailAction = "Detail";
        public const string StoriesAction = "Stories";
        public const string StartDateParam = "StartDate";
        public const string EndDateParam = "EndDate";
        public const string StartDateSqlParam = "@StartDate";
        public const string EndDateSqlParam = "@EndDate";
        public const string ProductCategoryIdSqlParam = "@ProductCategoryId";
        public const string EventLevelColumn = "EventLevel";
        public const string HomeIndexMenuLink = "home-index";
        public const string ProductsIndexMenuLink = "products-index";
        public const string CustomerNotFoundMessage = "Customer not found.";
        public const string PaymentResultIsNullMessage = "paymentResult is null";
        public const string ControllersNamespace = "EImece.Controllers";
        public const string StoriesRoute = "stories";
        public const string RobotsUserAgentAll = "User-agent: *";
        public const string GetAllActiveTemplatesCacheKey = "GetAllActiveTemplates";
        public const string DbEntityValidationExceptionPrefix = "DbEntityValidationException:";
        public const string ErrorResult = "error";
        public const string FieldNameColumn = "FieldName";
        public const string ValueFirstColumn = "ValueFirst";
        public const string ValueLastColumn = "ValueLast";
        public const string StatusMessageKey = "StatusMessage";
        public const string ActiveCssClass = "active";
        public const string TextHtmlUtf8ContentType = "text/html; charset=utf-8";
    }
}