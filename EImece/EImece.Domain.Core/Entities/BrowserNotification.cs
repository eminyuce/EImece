namespace EImece.Domain.Core.Entities;

public class BrowserNotification : BaseEntity
{
    public int NotificationType { get; set; }
    public string? ImageUrl { get; set; }
    public string? RedirectionUrl { get; set; }
}
