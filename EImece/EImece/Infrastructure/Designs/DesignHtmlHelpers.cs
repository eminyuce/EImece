using System;
using System.Web.Mvc;

namespace EImece.Infrastructure.Designs
{
    public static class DesignHtmlHelpers
    {
        private static IDesignProvider _designProvider = new ConfigDesignProvider();

        public static void SetDesignProvider(IDesignProvider provider)
        {
            _designProvider = provider ?? new ConfigDesignProvider();
        }

        public static string DesignAsset(this HtmlHelper html, string relativeAssetPath)
        {
            if (string.IsNullOrEmpty(relativeAssetPath))
            {
                return string.Empty;
            }

            string activeDesign = _designProvider.GetActiveDesign();
            string cleanAsset = relativeAssetPath.TrimStart('~', '/');

            if (string.IsNullOrEmpty(activeDesign))
            {
                return $"~/Content/{cleanAsset}";
            }

            return $"~/Content/designs/{activeDesign.ToLowerInvariant()}/{cleanAsset}";
        }
    }
}
