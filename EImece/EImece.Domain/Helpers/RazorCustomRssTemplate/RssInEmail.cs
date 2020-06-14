using System.Collections.Generic;

namespace EImece.Domain.Helpers.RazorCustomRssTemplate
{
    public class RssInEmail
    {
        public string rssUrl { get; set; }
        public bool isSubjectSource { get; set; }
        public List<SI> items { get; set; }
    }
}