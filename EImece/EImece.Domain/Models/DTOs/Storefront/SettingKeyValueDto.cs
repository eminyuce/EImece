namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal key/value projection for settings list pages where lookup by key is required.
    /// Query: SELECT SettingKey, SettingValue FROM Settings WHERE Lang=@lang (2 cols)
    /// </summary>
    public class SettingKeyValueDto
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }
}
