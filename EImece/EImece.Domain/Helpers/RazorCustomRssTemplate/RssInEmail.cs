using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.RazorCustomRssTemplate
{
    public class RssInEmail
    {
        public string rssUrl { get; set; }
        public bool isSubjectSource { get; set; }
        public List<SI> items { get; set; }
    }
}
