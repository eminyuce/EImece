using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class BrowserNotification : BaseEntity
    {
        public int NotificationType { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string RedirectionUrl { get; set; }
    }
}
