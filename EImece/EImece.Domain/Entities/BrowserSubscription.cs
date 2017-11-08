using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class BrowserSubscription : BaseEntity
    {
        public string Subject { get; set; }
        public int BrowserType { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
