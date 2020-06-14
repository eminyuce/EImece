namespace EImece.Domain.Entities
{
    public class BrowserSubscriber : BaseEntity
    {
        public int BrowserSubscriptionId { get; set; }
        public string EndPoint { get; set; }
        public string Auth { get; set; }
        public string P256dh { get; set; }
        public string UserAgent { get; set; }
        public string UserAddress { get; set; }

        public BrowserSubscription BrowserSubscription { get; set; }
    }
}