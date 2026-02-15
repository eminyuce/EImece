using System;

namespace EImece.Domain.Models.DTOs
{
    public class BrowserSubscriberDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BrowserSubscriber
        public int BrowserSubscriptionId { get; set; }
        public string EndPoint { get; set; }
        public string Auth { get; set; }
        public string P256dh { get; set; }
        public string UserAgent { get; set; }
        public string UserAddress { get; set; }
    }
}
