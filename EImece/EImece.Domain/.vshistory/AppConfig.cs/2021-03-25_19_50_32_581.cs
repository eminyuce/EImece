using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace EImece.Domain
{
    public static class AppConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string GetDefaultImage(int w, int h)
        {
            return GetDefaultImage($"w{w}h{h}");
        }

        public static string GetDefaultImage(String imageSize)
        {
            return $"/images/defaultimage/{imageSize}/default.jpg";
        }

        public static string IyzicoBaseUrl
        {
            get
            {
                return GetConfigString("IyzicoBaseUrl", "https://sandbox-api.iyzipay.com");
            }
        }

        public static string IyzicoSecretKey
        {
            get
            {
                return GetConfigString("IyzicoSecretKey", "sandbox-xi3ZmT9EVV0AcwaV4mzT4TlOmWr5YGgL");
            }
        }

        public static string IyzicoApiKey
        {
            get
            {
                return GetConfigString("IyzicoApiKey", "sandbox-v0nW7JMLDP8x5ZjVN2MQpKkcmKlUqKZB");
            }
        }

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
               // return HttpContext.Current.Request.Url.Scheme;
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
                return GetConfigInt("GridPageSizeNumber", 100);
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

        public static int SpecialFooterMenuLinkMenuId
        {
            get
            {
                return GetConfigInt("SpecialFooterMenuLinkMenuId", 3142);
            }
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

        public static int CacheVeryLongSeconds
        {
            get
            {
                return GetConfigInt("CacheVeryLongSeconds", 180000);
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
            get { return Path.Combine(HostingEnvironment.MapPath(Constants.ServerMapPath)); }
        }

        public static bool IsCacheActive
        {
            get
            {
                return GetConfigBool("IsCacheActive", true);
            }
        }

        public static bool IsImageFullSrcUnderMediaFolder
        {
            get
            {
                return GetConfigBool("IsImageFullSrcUnderMediaFolder", true);
            }
        }

        public static int ProductDefaultRecordPerPage
        {
            get { return 24; }
        }

        public static List<int> IyzicoEnabledInstallments
        {
            get
            {
                var IyzicoEnabledInstallmentsStr = GetConfigString("IyzicoEnabledInstallments", "1,2,4,6,9");
                List<int> enabledInstallments = new List<int>();
                foreach (var item in IyzicoEnabledInstallmentsStr.Split(",".ToCharArray()))
                {
                    enabledInstallments.Add(item.ToInt());
                }
                return enabledInstallments;
            }
        }

        public static string ShoppingCartItemCategory2
        {
            get
            {
                return GetConfigString("ShoppingCartItemCategory2", "ShoppingCartItemCategory2");
            }
        }

        public static bool IsSiteUnderConstruction
        {
            get
            {
                return GetConfigBool("IsSiteUnderConstruction", false);
            }
        }

        public static bool IsSiteLive
        {
            get
            {
                String siteStatus = AppConfig.GetConfigString("SiteStatus", "dev");
                return string.Equals(siteStatus, "live", StringComparison.InvariantCultureIgnoreCase);
            }
        }
        public static bool IsSiteUnderDevelopment
        {
            get
            {
                return !IsSiteLive;
            }
        }
    }
}