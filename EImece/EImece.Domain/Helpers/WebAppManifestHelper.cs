using EImece.Domain.Models.FrontModels;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Builds a W3C Web App Manifest JSON document from storefront branding values.
    /// Structural defaults come from <see cref="AppConfig"/> / Web.config (Manifest* keys).
    /// </summary>
    public static class WebAppManifestHelper
    {
        private static readonly int[] IconSizes = { 36, 48, 72, 96, 144, 192, 256, 384, 512 };
        private static readonly Regex HexColorRegex = new Regex(
            "^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

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
            var name = FirstNonEmpty(companyName, siteIndexMetaTitle, HostFromDomain(domainFallback), AppConfig.ManifestFallbackName);
            var shortName = ToShortName(name);
            var description = FirstNonEmpty(siteIndexMetaDescription, name);

            return new WebAppManifest
            {
                Name = name,
                ShortName = shortName,
                Description = description,
                StartUrl = AppConfig.ManifestStartUrl,
                Display = AppConfig.ManifestDisplay,
                Orientation = AppConfig.ManifestOrientation,
                ThemeColor = ResolveThemeColor(themeColorFromSettings, themeColorFallback),
                BackgroundColor = AppConfig.ManifestBackgroundColor,
                Icons = CreateIcons()
            };
        }

        public static string ResolveThemeColor(string fromSettings, string fromAppConfig)
        {
            if (IsValidHexColor(fromSettings))
            {
                return fromSettings.Trim();
            }

            if (IsValidHexColor(fromAppConfig))
            {
                return fromAppConfig.Trim();
            }

            return AppConfig.ManifestDefaultThemeColor;
        }

        public static bool IsValidHexColor(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && HexColorRegex.IsMatch(value.Trim());
        }

        public static string ToShortName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return AppConfig.ManifestFallbackName;
            }

            var trimmed = name.Trim();
            var maxLength = AppConfig.ManifestShortNameMaxLength;
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
