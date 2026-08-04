namespace EImece.Domain.Core.Entities;

public class BrowserNotificationFeedBack : BaseEntity
{
    public int BrowserNotificationId { get; set; }
    public int BrowserSubscriberId { get; set; }
    public int NotificationStatus { get; set; }
    public DateTime DateSend { get; set; }
    public DateTime? DateTracked { get; set; }
}
