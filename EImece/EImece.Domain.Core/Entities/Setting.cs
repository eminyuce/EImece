namespace EImece.Domain.Core.Entities;

public class Setting : BaseEntity
{
    public string? Description { get; set; }
    public string? SettingKey { get; set; }
    public string? SettingValue { get; set; }
}
