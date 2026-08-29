using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace EImece.Domain
{
    /// <summary>
    /// Static Web.config / infrastructure settings only.
    /// Dynamic business and admin settings are managed via ISettingService.
    /// </summary>
    public static class AppConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string GetDefaultImage(int w, int h)
        {
            return GetDefaultImage($"w{w}h{h}");
        }

        public static string GetDefaultImage(string imageSize)
        {
            return $"/images/defaultimage/{imageSize}/default.jpg";
        }

        /// <summary>
        /// Google reCAPTCHA v2 secret key (private). Configure via Web.config RecaptchaSecretKey.
        /// </summary>
        public static string RecaptchaSecretKey
        {
            get
            {
                return GetConfigString("RecaptchaSecretKey", string.Empty);
            }
        }

        /// <summary>
        /// Google reCAPTCHA v2 siteverify endpoint. Override via Web.config RecaptchaSiteVerifyUrl.
        /// </summary>
        public static string RecaptchaSiteVerifyUrl
        {
            get
            {
                return GetConfigString("RecaptchaSiteVerifyUrl", "https://www.google.com/recaptcha/api/siteverify");
            }
        }

        public static string IyzicoBaseUrl
        {
            get
            {
                return GetConfigString("IyzicoBaseUrl", "https://sandbox-api.iyzipay.com");
            }
        }

        /// <summary>
        /// Iyzico secret key. Read from environment variable EIMECE_IYZICO_SECRET_KEY, then Web.config / AppSettings.
        /// </summary>
        public static string IyzicoSecretKey
        {
            get
            {
                var fromEnv = Environment.GetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY");
                if (!string.IsNullOrWhiteSpace(fromEnv))
                {
                    return fromEnv.Trim();
                }

                return GetConfigString("IyzicoSecretKey", string.Empty);
            }
        }

        /// <summary>
        /// Iyzico API key. Read from environment variable EIMECE_IYZICO_API_KEY, then Web.config / AppSettings.
        /// </summary>
        public static string IyzicoApiKey
        {
            get
            {
                var fromEnv = Environment.GetEnvironmentVariable("EIMECE_IYZICO_API_KEY");
                if (!string.IsNullOrWhiteSpace(fromEnv))
                {
                    return fromEnv.Trim();
                }

                return GetConfigString("IyzicoApiKey", string.Empty);
            }
        }

        /// <summary>
        /// True when environment variables or AppSettings contain non-empty Iyzico credentials.
        /// </summary>
        public static bool HasConfiguredIyzicoCredentials
        {
            get
            {
                return !string.IsNullOrWhiteSpace(IyzicoApiKey) && !string.IsNullOrWhiteSpace(IyzicoSecretKey);
            }
        }

        /// <summary>
        /// Validates that payment credentials are configured when required.
        /// Fails closed with ConfigurationErrorsException if Iyzico keys are missing.
        /// </summary>
        public static void ValidatePaymentGatewayCredentials()
        {
            if (string.IsNullOrWhiteSpace(IyzicoApiKey) || string.IsNullOrWhiteSpace(IyzicoSecretKey))
            {
                throw new ConfigurationErrorsException(
                    "Payment gateway credentials are missing. " +
                    "Set environment variables 'EIMECE_IYZICO_API_KEY' and 'EIMECE_IYZICO_SECRET_KEY', " +
                    "or configure 'IyzicoApiKey' and 'IyzicoSecretKey' in AppSettings.");
            }
        }

        public static bool IsSmtpClientEnabled
        {
            get
            {
                return GetConfigBool("IsSmtpClientEnabled", true);
            }
        }

        public static string HttpProtocolForImages
        {
            get
            {
                return GetConfigString("HttpProtocolForImages", "http");
            }
        }

        public static bool UseSSL
        {
            get
            {
                return GetConfigBool("UseSSL", false);
            }
        }

        public static string HttpProtocol
        {
            get
            {
                return UseSSL ? "https" : "http";
            }
        }

        public static string Domain
        {
            get
            {
                return GetConfigString("domain", "127.0.0.1:81");
            }
        }

        public static int CacheTinySeconds
        {
            get { return 10; }
        }

        public static int CacheShortSeconds
        {
            get { return 60; }
        }

        public static int CacheMediumSeconds
        {
            get { return 300; }
        }

        public static int CacheLongSeconds
        {
            get { return 900; }
        }

        public static int CacheVeryLongSeconds
        {
            get { return 86400; }
        }

        public static bool IsEditLinkEnable
        {
            get
            {
                return GetConfigBool("IsEditLinkEnable", true);
            }
        }

        public static bool ShowThemeSelectionInAdmin
        {
            get
            {
                return GetConfigBool("ShowThemeSelectionInAdmin", true);
            }
        }

        public static int HomePageMainProductCountRandomLimit
        {
            get
            {
                return GetConfigInt("HomePageMainProductCountRandomLimit", 100);
            }
        }

        public static int HomePageMainProductCountLimit
        {
            get
            {
                return GetConfigInt("HomePageMainProductCountLimit", 12);
            }
        }

        public static int HomePageFeatureStoryCountLimit
        {
            get
            {
                return GetConfigInt("HomePageFeatureStoryCountLimit", 6);
            }
        }

        public static int RecordPerPage
        {
            get
            {
                return GetConfigInt("RecordPerPage", 20);
            }
        }

        public static int ProductDefaultRecordPerPage
        {
            get { return 24; }
        }

        public static int ProductCommentsRecordPerPage
        {
            get { return 8; }
        }

        public static int MaxItemsCountInFilter
        {
            get
            {
                return GetConfigInt("MaxItemsCountInFilter", 10);
            }
        }

        public static string ApplicationLanguages
        {
            get
            {
                return GetConfigString("ApplicationLanguages", "1,2");
            }
        }

        public static int MainLanguage
        {
            get
            {
                return GetConfigInt("MainLanguage", 1);
            }
        }

        public static bool IsMainLanguageSet
        {
            get
            {
                return MainLanguage > 0;
            }
        }

        public static bool IsImageFullSrcUnderMediaFolder
        {
            get
            {
                return GetConfigBool("IsImageFullSrcUnderMediaFolder", true);
            }
        }

        public static string ShoppingCartItemCategory2
        {
            get
            {
                return GetConfigString("ShoppingCartItemCategory2", "ShoppingCartItemCategory2");
            }
        }

        public static int MusteriIliskileriChildrenIdTurkce
        {
            get
            {
                return GetConfigInt("MusteriIliskileriChildrenIdTurkce", 25);
            }
        }

        public static int AdminImageHeightPercantage
        {
            get
            {
                return GetConfigInt("AdminImageHeightPercantage", 65);
            }
        }

        public static int AdminImageWidthPercantage
        {
            get
            {
                return GetConfigInt("AdminImageWidthPercantage", 65);
            }
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
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

        public static bool IsSiteLive
        {
            get
            {
                string siteStatus = GetConfigString("SiteStatus", "dev");
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

        /// <summary>
        /// When false, AdminLogin is unavailable and unauthenticated /admin requests redirect to the site home.
        /// </summary>
        public static bool AdminLoginEnabled
        {
            get
            {
                return GetConfigBool("AdminLoginEnabled", true);
            }
        }


        public static string DummyIdentityNumber
        {
            get
            {
                return GetConfigString("DummyIdentityNumber", "83312007240");
            }
        }

        private static readonly ConcurrentDictionary<string, string> StringCache =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, bool> BoolCache =
            new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, int> IntCache =
            new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, byte> LoggedFallbackKeys =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, byte> LoggedCacheMissKeys =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, byte> LoggedFirstHitKeys =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private const int CacheHitSummaryInterval = 100;

        internal static int FallbackLogCount;
        internal static int CacheHitCount;
        internal static int CacheMissCount;

        /// <summary>
        /// Clears resolved AppSettings cache. AppDomain recycle already does this; tests call it between cases.
        /// </summary>
        internal static void ResetCacheForTests()
        {
            StringCache.Clear();
            BoolCache.Clear();
            IntCache.Clear();
            LoggedFallbackKeys.Clear();
            LoggedCacheMissKeys.Clear();
            LoggedFirstHitKeys.Clear();
            FallbackLogCount = 0;
            CacheHitCount = 0;
            CacheMissCount = 0;
        }

        /// <summary>
        /// Warms frequently-read keys and writes one Info snapshot of missing AppSettings (fallbacks).
        /// Call once from Application_Start. Subsequent property access is cache-only and silent.
        /// </summary>
        public static void LogStartupSnapshot()
        {
            Touch(HomePageMainProductCountLimit);
            Touch(HomePageFeatureStoryCountLimit);
            Touch(HomePageMainProductCountRandomLimit);
            Touch(RecordPerPage);
            Touch(ApplicationLanguages);
            Touch(MainLanguage);
            Touch(IsCacheActive);
            Touch(IsSiteLive);
            Touch(IsSmtpClientEnabled);
            Touch(IsEditLinkEnable);
            Touch(Domain);
            Touch(GetConfigBool("EnableServiceMethodMetrics", true));
            Touch(GetConfigString("OtelServiceVersion", "1.0.0"));
            Touch(GetConfigInt(Constants.PerfStatsRetentionHours, Constants.DefaultPerfStatsRetentionHours));

            var fallbackKeys = LoggedFallbackKeys.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToArray();
            if (fallbackKeys.Length == 0)
            {
                Logger.Info(string.Format(
                    CultureInfo.InvariantCulture,
                    "AppConfig resolved. All warmed keys are present in AppSettings. Cache misses={0} hits={1} stringKeys={2} intKeys={3} boolKeys={4}",
                    CacheMissCount,
                    CacheHitCount,
                    StringCache.Count,
                    IntCache.Count,
                    BoolCache.Count));
                return;
            }

            Logger.Info(string.Format(
                CultureInfo.InvariantCulture,
                "AppConfig resolved. {0} key(s) using defaults (logged once): {1}. Cache misses={2} hits={3} stringKeys={4} intKeys={5} boolKeys={6}",
                fallbackKeys.Length,
                string.Join(", ", fallbackKeys),
                CacheMissCount,
                CacheHitCount,
                StringCache.Count,
                IntCache.Count,
                BoolCache.Count));
        }

        public static string GetConfigString(string configName, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(configName))
            {
                return defaultValue;
            }

            string cached;
            if (StringCache.TryGetValue(configName, out cached))
            {
                LogCacheHit("string", configName);
                return cached;
            }

            LogCacheMiss("string", configName);
            return StringCache.GetOrAdd(configName, _ => ResolveString(configName, defaultValue));
        }

        public static bool GetConfigBool(string configName, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(configName))
            {
                return defaultValue;
            }

            bool cached;
            if (BoolCache.TryGetValue(configName, out cached))
            {
                LogCacheHit("bool", configName);
                return cached;
            }

            LogCacheMiss("bool", configName);
            return BoolCache.GetOrAdd(configName, _ => ResolveBool(configName, defaultValue));
        }

        public static int GetConfigInt(string configName, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(configName))
            {
                return defaultValue;
            }

            int cached;
            if (IntCache.TryGetValue(configName, out cached))
            {
                LogCacheHit("int", configName);
                return cached;
            }

            LogCacheMiss("int", configName);
            return IntCache.GetOrAdd(configName, _ => ResolveInt(configName, defaultValue));
        }

        private static string ResolveString(string configName, string defaultValue)
        {
            var appValue = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(appValue))
            {
                LogFallbackOnce(configName, defaultValue);
                return defaultValue;
            }

            Logger.Debug(CultureInfo.InvariantCulture, "AppConfig '{0}' = '{1}'", configName, appValue);
            return appValue;
        }

        private static bool ResolveBool(string configName, bool defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(raw))
            {
                LogFallbackOnce(configName, defaultValue);
                return defaultValue;
            }

            return raw.ToBool();
        }

        private static int ResolveInt(string configName, int defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(raw))
            {
                LogFallbackOnce(configName, defaultValue);
                return defaultValue;
            }

            int configValue = raw.ToInt();
            if (configValue == -1)
            {
                LogFallbackOnce(configName, defaultValue);
                return defaultValue;
            }

            return configValue;
        }

        /// <summary>
        /// Logs a missing AppSettings key at Debug, and only the first time per AppDomain.
        /// Startup <see cref="LogStartupSnapshot"/> raises a single Info summary instead of per-request Info.
        /// </summary>
        private static void LogFallbackOnce(string configName, object defaultValue)
        {
            if (!LoggedFallbackKeys.TryAdd(configName, 0))
            {
                return;
            }

            System.Threading.Interlocked.Increment(ref FallbackLogCount);
            Logger.Debug(string.Format(
                CultureInfo.InvariantCulture,
                "Config '{0}' is using default value '{1}'. Add <add key=\"{0}\" value=\"{1}\" /> to AppSettings to override.",
                configName,
                defaultValue));
        }

        private static void LogCacheMiss(string kind, string configName)
        {
            System.Threading.Interlocked.Increment(ref CacheMissCount);
            var logKey = kind + ":" + configName;
            if (!LoggedCacheMissKeys.TryAdd(logKey, 0))
            {
                Logger.Debug(string.Format(
                    CultureInfo.InvariantCulture,
                    "AppConfig CACHE MISS (concurrent) [{0}] '{1}'",
                    kind,
                    configName));
                return;
            }

            Logger.Debug(string.Format(
                CultureInfo.InvariantCulture,
                "AppConfig CACHE MISS [{0}] '{1}' — reading AppSettings (first resolve).",
                kind,
                configName));
        }

        private static void LogCacheHit(string kind, string configName)
        {
            var hits = System.Threading.Interlocked.Increment(ref CacheHitCount);
            var logKey = kind + ":" + configName;
            if (LoggedFirstHitKeys.TryAdd(logKey, 0))
            {
                Logger.Debug(string.Format(
                    CultureInfo.InvariantCulture,
                    "AppConfig CACHE HIT [{0}] '{1}' — served from memory (caching is working).",
                    kind,
                    configName));
                return;
            }

            Logger.Debug(string.Format(
                CultureInfo.InvariantCulture,
                "AppConfig CACHE HIT [{0}] '{1}' (totalHits={2})",
                kind,
                configName,
                hits));

            if (hits % CacheHitSummaryInterval == 0)
            {
                Logger.Debug(string.Format(
                    CultureInfo.InvariantCulture,
                    "AppConfig cache stats: hits={0} misses={1} stringKeys={2} intKeys={3} boolKeys={4}",
                    hits,
                    CacheMissCount,
                    StringCache.Count,
                    IntCache.Count,
                    BoolCache.Count));
            }
        }

        private static void Touch<T>(T value)
        {
            System.GC.KeepAlive(value);
        }
    }
}