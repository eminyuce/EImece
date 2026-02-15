using System;

namespace EImece.Domain.Models.DTOs
{
    public class BrowserNotificationFeedBackDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BrowserNotificationFeedBack
        public int BrowserNotificationId { get; set; }
        public int BrowserSubscriberId { get; set; }
        public int NotificationStatus { get; set; }
        public DateTime DateSend { get; set; }
        public DateTime? DateTracked { get; set; }
    }
}
