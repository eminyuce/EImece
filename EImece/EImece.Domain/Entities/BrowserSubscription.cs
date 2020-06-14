using EImece.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class BrowserSubscription : BaseEntity
    {
        public string Subject { get; set; }
        public int BrowserType { get; set; }

        [NotMapped]
        public BrowserType BrowserTypeEnum { get { return (BrowserType)BrowserType; } set { BrowserType = (int)value; } }

        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}