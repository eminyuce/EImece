using EImece.Domain.Models.Enums;
using System;

namespace EImece.Domain.Models.DTOs
{
    public class BrowserSubscriptionDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BrowserSubscription
        public string Subject { get; set; }
        public int BrowserType { get; set; }
        public BrowserType BrowserTypeEnum { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
