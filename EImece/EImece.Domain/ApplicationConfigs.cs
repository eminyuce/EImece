using EImece.Domain.Helpers;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace EImece.Domain
{
    public static class ApplicationConfigs
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
        //   public const string Languages = "Languages";
        public const string PrivacyPolicy = "PrivacyPolicy";
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

        public static string AdminCultureCookieName = "_adminCulture";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string HttpProtocolForImages
        {
            get
            {
                var useSll = GetConfigBool("UseSSLImages", false);
                return string.Format("http{0}", useSll ? "s" : ""); 
                // return HttpContext.Current.Request.Url.Scheme;
            }
        }

        public static string HttpProtocol
        {
            get
            {
                var useSll = GetConfigBool("UseSSL", false);
                return string.Format("http{0}", useSll ? "s" : "");
                //return HttpContext.Current.Request.Url.Scheme;
            }
        }

        public static object DeleteButtonHtmlAttribute
        {
            get
            {
                return new { @class = "btn btn-sm btn-danger   glyphicon glyphicon-trash glyphicon-white" }
;
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

        public static string Domain
        {
            get
            {
                return GetConfigString("domain");
            }
        }

        public static int GridPageSizeNumber
        {
            get
            {
                return GetConfigInt("GridPageSizeNumber", 200);
            }
        }

        //en-US,tr-TR
        public static string ApplicationLanguages
        {
            get
            {
                return GetConfigString("Application_Languages");
            }
        }

        public static int RecordPerPage
        {
            get { return 24; }
        }

        public static int MaxItemsCountInFilter
        {
            get { return 20; }
        }

        private static void WriteLog(string configName, object defaultValue)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Any(r => r.Equals(configName, StringComparison.InvariantCultureIgnoreCase)) && defaultValue != null)
            {
                    Logger.Info(string.Format("Config Name {0} is using default value {1}      <add key=\"{0}\" value=\"{1}\" />", configName, defaultValue));
            }
        }

        public static string GetConfigString(string configName, string defaultValue = "")
        {
            var appValue = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(appValue))
            {
                WriteLog(configName, defaultValue);
                return defaultValue;
            }
            else
            {
                return appValue;
            }
        }

        public static bool GetConfigBool(string configName, bool defaultValue = false)
        {
            var configValue = defaultValue;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[configName]))
            {
                configValue = ConfigurationManager.AppSettings[configName].ToBool();
            }
            else
            {
                WriteLog(configName, defaultValue);
            }
            return configValue;
        }

        public static int GetConfigInt(string configName, int defaultValue = 0)
        {
            int configValue = -1;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[configName]))
            {
                configValue = ConfigurationManager.AppSettings[configName].ToInt();
            }
            else
            {
                WriteLog(configName, defaultValue);
            }
            return configValue == -1 ? defaultValue : configValue;
        }

        public static int CacheTinySeconds
        {
            get
            {
                return GetConfigInt("CacheTinySeconds", 1);
            }
        }

        public static int CacheShortSeconds
        {
            get
            {
                return GetConfigInt("CacheShortSeconds", 10);
            }
        }

        public static int CacheMediumSeconds
        {
            get
            {
                return GetConfigInt("CacheMediumSeconds", 300);
            }
        }

        public static int CacheLongSeconds
        {
            get
            {
                return GetConfigInt("CacheLongSeconds", 1800);
            }
        }
 
        public static bool IsEditLinkEnable
        {
            get
            {
                return GetConfigBool("IsEditLinkEnable", true);
            }
        }

        public static bool IsDebug
        {
            get
            {
                var isDebug = false;
#if DEBUG
                isDebug = true;
#endif
                return isDebug;
            }
        }

        public static bool IsMainLanguageSet
        {
            get
            {
                return MainLanguage > 0;
            }
        }

        public static int MainLanguage
        {
            get
            {
                return GetConfigInt("MainLanguage", 1);
            }
        }

        public static string StorageRoot
        {
            get { return Path.Combine(HostingEnvironment.MapPath(ServerMapPath)); }
        }

        public static bool IsCacheActive
        {
            get
            {
                return GetConfigBool("IsCacheActive", true);
            }
        }

    }
}