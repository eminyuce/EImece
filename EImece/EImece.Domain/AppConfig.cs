using EImece.Domain.Helpers;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;

namespace EImece.Domain
{
    /// <summary>
    /// Static Web.config / infrastructure settings only.
    /// Dynamic business and admin settings are managed via ISettingService.
    /// </summary>
    public static class AppConfig
    {
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

        /// <summary>
        /// Enabled content-language cultures (tr-TR and/or en-US), from the SupportedContentLanguages system setting.
        /// Defaults to Turkish only when the setting is missing.
        /// </summary>
        public static string ApplicationLanguages
        {
            get
            {
                return ContentLanguageSettingsHelper.GetCurrent().SerializedCultures;
            }
        }

        /// <summary>
        /// Default content language id. English-only sites return English; otherwise Turkish.
        /// </summary>
        public static int MainLanguage
        {
            get
            {
                return ContentLanguageSettingsHelper.GetCurrent().DefaultLanguageId;
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
            get
            {
                var serverMap = Constants.ServerMapPath?.TrimStart('~', '/', '\\') ?? Path.Combine("media", "images");
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serverMap);
            }
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

        /// <summary>
        /// Clears resolved AppSettings cache. AppDomain recycle already does this; tests call it between cases.
        /// </summary>
        internal static void ResetCacheForTests()
        {
            StringCache.Clear();
            BoolCache.Clear();
            IntCache.Clear();
        }

        /// <summary>
        /// Compatibility shim for older web binaries that still call this from Application_Start.
        /// </summary>
        public static void LogStartupSnapshot()
        {
            WarmCache();
        }

        /// <summary>
        /// Warms frequently-read keys so the first request does not pay AppSettings lookups.
        /// Call once from Application_Start.
        /// </summary>
        public static void WarmCache()
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
                return cached;
            }

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
                return cached;
            }

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
                return cached;
            }

            return IntCache.GetOrAdd(configName, _ => ResolveInt(configName, defaultValue));
        }

        private static string ResolveString(string configName, string defaultValue)
        {
            var appValue = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(appValue))
            {
                return defaultValue;
            }

            return appValue;
        }

        private static bool ResolveBool(string configName, bool defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(raw))
            {
                return defaultValue;
            }

            return raw.ToBool();
        }

        private static int ResolveInt(string configName, int defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(raw))
            {
                return defaultValue;
            }

            int configValue = raw.ToInt();
            if (configValue == -1)
            {
                return defaultValue;
            }

            return configValue;
        }

        private static void Touch<T>(T value)
        {
            System.GC.KeepAlive(value);
        }
    }
}