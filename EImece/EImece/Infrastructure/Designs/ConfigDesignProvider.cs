using System.Configuration;

namespace EImece.Infrastructure.Designs
{
    public class ConfigDesignProvider : IDesignProvider
    {
        public static string AppSettingsKey { get; set; } = "ActiveDesign";

        public string GetActiveDesign()
        {
            var design = ConfigurationManager.AppSettings[AppSettingsKey];
            return string.IsNullOrWhiteSpace(design) ? string.Empty : design.Trim();
        }
    }
}
