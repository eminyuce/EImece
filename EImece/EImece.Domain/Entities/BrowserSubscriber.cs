using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources;

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
