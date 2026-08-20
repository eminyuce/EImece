using EImece.Domain;
using System.Configuration;

namespace EImece.Infrastructure.Designs
{
    public class ConfigDesignProvider : IDesignProvider
    {
        public static string AppSettingsKey { get; set; } = "ActiveDesign";

        public string GetActiveDesign()
        {
            var design = AppConfig.ActiveDesign;
            if (!string.IsNullOrWhiteSpace(design))
            {
                return design.Trim();
            }

            var appSettingDesign = ConfigurationManager.AppSettings[AppSettingsKey];
            return string.IsNullOrWhiteSpace(appSettingDesign) ? string.Empty : appSettingDesign.Trim();
        }
    }
}
