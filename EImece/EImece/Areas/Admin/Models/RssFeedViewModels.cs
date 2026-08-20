using System.Collections.Generic;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Models
{
    public class RssFeedParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
    }

    public class RssFeedInfo
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string RelativePath { get; set; }
        public string HttpMethod { get; set; } = "GET";
        public string ContentType { get; set; } = "application/rss+xml";
        public string CacheDuration { get; set; } = "20 Dakika (CustomOutputCache)";
        public string OutputFormat { get; set; }
        public bool RequiresCategoryId { get; set; }
        public string ControllerAction { get; set; }
        public string DefaultSampleQuery { get; set; }
        public List<RssFeedParameterInfo> Parameters { get; set; } = new List<RssFeedParameterInfo>();
    }

    public class RssFeedsIndexViewModel
    {
        public string BaseUrl { get; set; }
        public List<RssFeedInfo> Feeds { get; set; } = new List<RssFeedInfo>();
        public List<SelectListItem> StoryCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProductCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Languages { get; set; } = new List<SelectListItem>();
    }
}
