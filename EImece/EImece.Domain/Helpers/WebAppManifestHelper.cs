using EImece.Domain.DependencyInjection;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Builds a W3C Web App Manifest JSON document from storefront branding values.
    /// Structural defaults come from ISettingService / Constants.
    /// </summary>
    public static class WebAppManifestHelper
    {
        private static readonly int[] IconSizes = { 36, 48, 72, 96, 144, 192, 256, 384, 512 };
        private static readonly Regex HexColorRegex = new Regex(
            "^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static string GetSetting(string key, string defaultValue)
        {
            var settingService = DomainServiceProvider.GetService<ISettingService>();
            var val = settingService?.GetSettingByKey(key);
            return !string.IsNullOrWhiteSpace(val) ? val.Trim() : defaultValue;
        }

        private static int GetSettingInt(string key, int defaultValue)
        {
            var settingService = DomainServiceProvider.GetService<ISettingService>();
            var val = settingService?.GetSettingByKey(key);
            return !string.IsNullOrWhiteSpace(val) ? val.ToInt(defaultValue) : defaultValue;
        }

        public static string BuildJson(
            string companyName,
            string siteIndexMetaTitle,
            string siteIndexMetaDescription,
            string themeColorFromSettings,
            string themeColorFallback,
            string domainFallback)
        {
            var manifest = Build(
                companyName,
                siteIndexMetaTitle,
                siteIndexMetaDescription,
                themeColorFromSettings,
                themeColorFallback,
                domainFallback);

            return JsonConvert.SerializeObject(manifest, Formatting.Indented);
        }

        public static WebAppManifest Build(
            string companyName,
            string siteIndexMetaTitle,
            string siteIndexMetaDescription,
            string themeColorFromSettings,
            string themeColorFallback,
            string domainFallback)
        {
            var fallbackName = GetSetting(Constants.ManifestFallbackName, Constants.DefaultManifestFallbackName);
            var name = FirstNonEmpty(companyName, siteIndexMetaTitle, HostFromDomain(domainFallback), fallbackName);
            var shortName = ToShortName(name);
            var description = FirstNonEmpty(siteIndexMetaDescription, name);

            return new WebAppManifest
            {
                Name = name,
                ShortName = shortName,
                Description = description,
                StartUrl = GetSetting(Constants.ManifestStartUrl, Constants.DefaultManifestStartUrl),
                Display = GetSetting(Constants.ManifestDisplay, Constants.DefaultManifestDisplay),
                Orientation = GetSetting(Constants.ManifestOrientation, Constants.DefaultManifestOrientation),
                ThemeColor = ResolveThemeColor(themeColorFromSettings, themeColorFallback),
                BackgroundColor = GetSetting(Constants.ManifestBackgroundColor, Constants.DefaultManifestBackgroundColor),
                Icons = CreateIcons()
            };
        }

        public static string ResolveThemeColor(string fromSettings, string fromFallback)
        {
            if (IsValidHexColor(fromSettings))
            {
                return fromSettings.Trim();
            }

            if (IsValidHexColor(fromFallback))
            {
                return fromFallback.Trim();
            }

            return GetSetting(Constants.ThemeColor, Constants.DefaultThemeColor);
        }

        public static bool IsValidHexColor(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && HexColorRegex.IsMatch(value.Trim());
        }

        public static string ToShortName(string name)
        {
            var fallbackName = GetSetting(Constants.ManifestFallbackName, Constants.DefaultManifestFallbackName);
            if (string.IsNullOrWhiteSpace(name))
            {
                return fallbackName;
            }

            var trimmed = name.Trim();
            var maxLength = GetSettingInt(Constants.ManifestShortNameMaxLength, Constants.DefaultManifestShortNameMaxLength);
            if (trimmed.Length <= maxLength)
            {
                return trimmed;
            }

            var slice = trimmed.Substring(0, maxLength).TrimEnd();
            var lastSpace = slice.LastIndexOf(' ');
            if (lastSpace >= 4)
            {
                return slice.Substring(0, lastSpace).TrimEnd();
            }

            return slice;
        }

        public static string HostFromDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return string.Empty;
            }

            var host = domain.Trim();
            var colon = host.IndexOf(':');
            if (colon > 0)
            {
                host = host.Substring(0, colon);
            }

            return host;
        }

        public static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static WebAppManifestIcon[] CreateIcons()
        {
            var icons = new WebAppManifestIcon[IconSizes.Length];
            for (var i = 0; i < IconSizes.Length; i++)
            {
                var size = IconSizes[i];
                var sizeLabel = size + "x" + size;
                icons[i] = new WebAppManifestIcon
                {
                    Src = "/android-chrome-" + sizeLabel + ".png",
                    Sizes = sizeLabel,
                    Type = "image/png"
                };
            }

            return icons;
        }
    }
}
