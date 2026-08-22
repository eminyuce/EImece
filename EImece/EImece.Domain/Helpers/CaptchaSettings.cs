using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using System;
using System.Web.Mvc;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Reads Captcha & Anti-Spam settings dynamically from ISettingService / database
    /// with constant default fallbacks.
    /// </summary>
    public static class CaptchaSettings
    {
        public static CaptchaProviderType Provider
        {
            get
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                var raw = settingService?.GetSettingByKey(Constants.CaptchaProvider);
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    if (Enum.TryParse(raw.Trim(), true, out CaptchaProviderType parsed))
                    {
                        return parsed;
                    }

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

                    if (raw.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        return CaptchaProviderType.None;
                    }
                }

                return CaptchaProviderType.Legacy;
            }
        }

        public static string RecaptchaSiteKey
        {
            get
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                return settingService?.GetSettingByKey(Constants.RecaptchaSiteKey) ?? string.Empty;
            }
        }

        public static bool RecaptchaEnabled => Provider == CaptchaProviderType.Recaptcha;

        public static bool IsLegacyCaptchaEnabled => Provider == CaptchaProviderType.Legacy;
    }
}
