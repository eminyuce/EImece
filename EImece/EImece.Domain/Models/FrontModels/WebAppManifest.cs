using Newtonsoft.Json;

namespace EImece.Domain.Models.FrontModels
{
    /// <summary>
    /// Web App Manifest payload served at /manifest.json.
    /// Property names follow the W3C manifest spec (snake_case JSON).
    /// </summary>
    public class WebAppManifest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("start_url")]
        public string StartUrl { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("orientation")]
        public string Orientation { get; set; }

        [JsonProperty("theme_color")]
        public string ThemeColor { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("icons")]
        public WebAppManifestIcon[] Icons { get; set; }
    }

    public class WebAppManifestIcon
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("sizes")]
        public string Sizes { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
