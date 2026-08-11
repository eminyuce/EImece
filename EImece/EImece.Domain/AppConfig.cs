using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Captcha implementation: Legacy (arithmetic image), Recaptcha (Google v2), or None.
        /// Default is Legacy for backward compatibility with the original CAPTCHA.
        /// </summary>
        public static CaptchaProviderType CaptchaProvider
        {
            get
            {
                var raw = GetConfigString("CaptchaProvider", string.Empty);
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    if (Enum.TryParse(raw.Trim(), true, out CaptchaProviderType parsed))
                    {
                        return parsed;
                    }

                    // Accept common aliases
                    if (raw.Equals("Arithmetic", StringComparison.OrdinalIgnoreCase)
                        || raw.Equals("Weak", StringComparison.OrdinalIgnoreCase)
                        || raw.Equals("Old", StringComparison.OrdinalIgnoreCase))
                    {
                        return CaptchaProviderType.Legacy;
                    }

                    if (raw.Equals("Google", StringComparison.OrdinalIgnoreCase)
                        || raw.Equals("GoogleRecaptcha", StringComparison.OrdinalIgnoreCase)
                        || raw.Equals("RecaptchaV2", StringComparison.OrdinalIgnoreCase))
                    {
                        return CaptchaProviderType.Recaptcha;
                    }

                    Logger.Warn($"Unknown CaptchaProvider value '{raw}'. Falling back to Legacy.");
                    return CaptchaProviderType.Legacy;
                }

                // Backward compatible with RecaptchaEnabled from earlier integration
                if (GetConfigBool("RecaptchaEnabled", false))
                {
                    return CaptchaProviderType.Recaptcha;
                }

                return CaptchaProviderType.Legacy;
            }
        }

        /// <summary>
        /// Google reCAPTCHA v2 site key (public). Configure via Web.config RecaptchaSiteKey.
        /// </summary>
        public static string RecaptchaSiteKey
        {
            get
            {
                return GetConfigString("RecaptchaSiteKey", string.Empty);
            }
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
        /// True when CaptchaProvider is Recaptcha. Kept for callers that previously checked RecaptchaEnabled.
        /// Prefer <see cref="CaptchaProvider"/>.
        /// </summary>
        public static bool RecaptchaEnabled
        {
            get
            {
                return CaptchaProvider == CaptchaProviderType.Recaptcha;
            }
        }

        /// <summary>
        /// True when CaptchaProvider is Legacy (original arithmetic CAPTCHA).
        /// </summary>
        public static bool IsLegacyCaptchaEnabled
        {
            get
            {
                return CaptchaProvider == CaptchaProviderType.Legacy;
            }
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
                return GetConfigString("IyzicoSecretKey", "lvpx3JoZMoUF9f0RNDoEsxDSMQUUlpWH");
            }
        }

        public static string IyzicoApiKey
        {
            get
            {
                return GetConfigString("IyzicoApiKey", "sandbox-v0nW7JMLDP8x5ZjVN2MQpKkcmKlUqKZB");
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
                return string.Format("http{0}", UseSSL ? "s" : "");
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
                return string.Format("http{0}", UseSSL ? "s" : "");
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

        public static int HomePageMainProductCountRandomLimit
        {
            get
            {
                return HomePageMainProductCountLimit / 3;
            }
        }

        public static int HomePageMainProductCountLimit
        {
            get
            {
                return GetConfigInt("HomePageMainProductCountLimit", 15);
            }
        }

        public static int HomePageFeatureStoryCountLimit
        {
            get
            {
                return GetConfigInt("HomePageFeatureStoryCountLimit", 1);
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

        public static int CacheVeryLongSeconds
        {
            get
            {
                return GetConfigInt("CacheVeryLongSeconds", 180000);
            }
        }

        public static int MusteriIliskileriChildrenIdTurkce
        {
            get
            {
                return GetConfigInt("MusteriIliskileriChildrenIdTurkce", 6149);
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

        public static int ProductCommentsRecordPerPage
        {
            get { return 8; }
        }

        /// <summary>
        /// Active payment provider strategy key (e.g. "Iyzico"). Used by DI to select <c>IPaymentStrategy</c>.
        /// </summary>
        public static string PaymentProvider
        {
            get
            {
                return GetConfigString("PaymentProvider", "Iyzico");
            }
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

        /// <summary>
        /// When true, unhandled exceptions show full stack traces (YSOD / detailed error pages)
        /// instead of the generic friendly 500 page. Defaults to true in non-live environments.
        /// Set appSetting ExposeDetailedErrors=true to force this even when SiteStatus=live (local IIS).
        /// Web.Release.config should keep this false for production.
        /// </summary>
        public static bool ExposeDetailedErrors
        {
            get
            {
                if (ConfigurationManager.AppSettings["ExposeDetailedErrors"] != null)
                {
                    return GetConfigBool("ExposeDetailedErrors", false);
                }

                return IsSiteUnderDevelopment;
            }
        }

        /// <summary>
        /// TEMPORARY debug switch: when true, admin auth/login is bypassed and a debug Admin principal is injected.
        /// Keep false in production (Web.Release.config forces false).
        /// Hard-disabled whenever SiteStatus indicates a live environment.
        /// </summary>
        public static bool BypassAdminAuth
        {
            get
            {
                if (IsSiteLive)
                {
                    return false;
                }

                return GetConfigBool("BypassAdminAuth", false);
            }
        }

        /// <summary>
        /// When false, AdminLogin is unavailable and unauthenticated /admin requests redirect to the site home
        /// instead of the login page. Set true to allow the normal admin login flow.
        /// </summary>
        public static bool AdminLoginEnabled
        {
            get
            {
                return GetConfigBool("AdminLoginEnabled", true);
            }
        }

        /// <summary>
        /// When true, admin/editor users must enable TOTP authenticator before using the admin panel.
        /// Defaults to true. Set false for temporary local work; Web.Release keeps true for production.
        /// Enforcement is also skipped when compilation debug is on, BypassAdminAuth is on,
        /// or the user is listed in TwoFactorBypassUsers.
        /// </summary>
        public static bool RequireAdminAuthenticator
        {
            get
            {
                return GetConfigBool("RequireAdminAuthenticator", true);
            }
        }

        /// <summary>
        /// Comma-separated emails/usernames that may use the admin panel without authenticator 2FA
        /// (e.g. a dedicated local debug account).
        /// </summary>
        public static string TwoFactorBypassUsers
        {
            get
            {
                return GetConfigString("TwoFactorBypassUsers", string.Empty);
            }
        }

        public static bool IsTwoFactorBypassUser(string emailOrUserName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName))
            {
                return false;
            }

            var raw = TwoFactorBypassUsers;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            var needle = emailOrUserName.Trim();
            foreach (var part in raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Equals(part.Trim(), needle, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string DummyIdentityNumber
        {
            get
            {
                return GetConfigString("DummyIdentityNumber", "83312007240");
            }
        }
    }
}