using EImece.Domain.Models.Enums;
using System;

namespace EImece.Domain.Models.DTOs
{
    public class BrowserNotificationDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BrowserNotification
        public int NotificationType { get; set; }
        public NotificationType NotificationTypeEnum { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string RedirectionUrl { get; set; }
        public bool IsSend { get; set; }
    }
}
