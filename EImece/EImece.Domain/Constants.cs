namespace EImece.Domain
{
    public static class Constants
    {
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
        public const string ProductsCategoriesControllerRoutingPrefix = "my_category";
        public const string ProductsControllerRoutingPrefix = "my_products";
        public const string SiteIndexMetaTitle = "SiteIndexMetaTitle";
        public const string IsProductPriceEnable = "IsProductPriceEnable";
        public const string SiteIndexMetaDescription = "SiteIndexMetaDescription";
        public const string SiteIndexMetaKeywords = "SiteIndexMetaKeywords";
        public const string SpecialPage = "Special_Page";
        public const string AdminSetting = "AdminSetting";
        public const string TermsAndConditions = "TermsAndConditions";
        public const string AboutUs = "AboutUs";
        public const string WebSiteLogo = "WebSiteLogo";
        public const string CompanyName = "CompanyName";
        public const string InstagramWebSiteLink = "InstagramWebSiteLink";
        public const string TwitterWebSiteLink = "TwitterWebSiteLink";
        public const string LinkedinWebSiteLink = "LinkedinWebSiteLink";
        public const string FacebookWebSiteLink = "FacebookWebSiteLink";
        public const string GoogleAnalyticsTrackingScript = "GoogleAnalyticsTrackingScript";
        public const string WebSiteCompanyPhoneAndLocation = "WebSiteCompanyPhoneAndLocation";

        public const string PrivacyPolicy = "PrivacyPolicy";
        public const string ELanguage = "ELanguage";

        public const string TempDataReturnUrlReferrer = "TempDataReturnUrlReferrer";

        public const string DbConnectionKey = "EImeceDbConnection";
        public const string AdministratorRole = "Admin";
        public const string EditorRole = "NormalUser";
        public const string ImageActionName = "Index";
        public const int PartialViewOutputCachingDuration = 86400;
        public const string SelectedLanguage = "SelectedLanguage";

        public const string TempPath = "~/media/tempFiles/";
        public const string ServerMapPath = "~/media/images/";
        public const string UrlBase = "/media/images/";
        public const string DeleteURL = "/Media/DeleteFile/?file={0}&contentId={1}&mod={2}&imageType={3}";
        public const string DeleteType = "GET";
        public const string FileUploadDeleteURL = "/FileUpload/DeleteFile/?file=";
        public const string AdminCultureCookieName = "_adminCulture";
        public const string CultureCookieName = "_culture";
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