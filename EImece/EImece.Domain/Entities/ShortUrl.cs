using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ShortUrl : BaseEntity
    {
        public string UrlKey { get; set; }
        public string Url { get; set; }
        public int RequestCount { get; set; }
    }
}
