using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain
{
    public class Settings
    {
        public static String DbConnectionKey = "EImeceDbConnection";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static String HttpProtocolForImages
        {
            get
            {
                var useSll = Settings.GetConfigBool("UseSSLImages", false);
                String httpSecure = String.Format("http{0}", useSll ? "s" : "");

                return httpSecure;
            }
        }
        public static String HttpProtocol
        {
            get
            {
                var useSll = GetConfigBool("UseSSL", false);
                String httpSecure = String.Format("http{0}", useSll ? "s" : "");

                return httpSecure;
            }
        }
        public static string OkStyle
        {
            get
            {

                return "style='color:green;font-size:2em;' class='glyphicon glyphicon-ok-circle'";

            }
        }
        public static string CancelStyle
        {
            get
            {

                return "style='color:red;  font-size:2em;' class='glyphicon  glyphicon-remove-circle'";

            }
        }
        public static string SiteName
        {
            get
            {
                return GetConfigString("siteName", "Maritime Survey");
            }
        }

        public static string Domain
        {
            get
            {
                return GetConfigString("domain");
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
        public static string GetConfigString(string configName, string defaultValue = "")
        {
            var appValue = ConfigurationManager.AppSettings[configName];
            if (String.IsNullOrEmpty(appValue))
            {
                Logger.Info(String.Format("Config Name {0} is using default value {1}      <add key=\"{0}\" value=\"{1}\" />", configName, defaultValue));
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
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings[configName]))
            {
                configValue = ConfigurationManager.AppSettings[configName].ToBool();
            }
            else
            {
                Logger.Info(String.Format("Config Name {0} is using default value {1}  <add key=\"{0}\" value=\"{1}\" />", configName, defaultValue));
            }
            return configValue;

        }

        public static int GetConfigInt(string configName, int defaultValue = 0)
        {
            int configValue = -1;
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings[configName]))
            {
                configValue = ConfigurationManager.AppSettings[configName].ToInt();
            }
            else
            {
                Logger.Info(String.Format("Config Name {0} is using default value {1}   <add key=\"{0}\" value=\"{1}\" />", configName, defaultValue));
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
        public static int Cache1Hour
        {
            get
            {
                return GetConfigInt("CacheLongSeconds", 3600);
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
    }
}
