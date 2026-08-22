namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal purpose-specific DTO for storefront settings where only the value column is required.
    /// Projection must be: SELECT SettingValue FROM Settings WHERE SettingKey=@key (single column).
    /// Do not add Id, Name, CreatedDate etc. — they are not consumed by the view.
    /// </summary>
    public class SettingValueDto
    {
        public string SettingValue { get; set; }
    }
}
