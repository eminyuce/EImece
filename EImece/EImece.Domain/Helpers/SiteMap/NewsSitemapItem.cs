using System;

namespace EImece.Domain.Helpers.SiteMap
{
    public class NewsSitemapItem : SitemapItem
    {
        /// <summary>
        /// URL of the page.
        /// </summary>
#pragma warning disable CS0108 // 'NewsSitemapItem.Url' hides inherited member 'SitemapItem.Url'. Use the new keyword if hiding was intended.
        public string Url { get; protected set; }
#pragma warning restore CS0108 // 'NewsSitemapItem.Url' hides inherited member 'SitemapItem.Url'. Use the new keyword if hiding was intended.

        /// <summary>
        /// The date of last modification of the file.
        /// </summary>
#pragma warning disable CS0108 // 'NewsSitemapItem.LastModified' hides inherited member 'SitemapItem.LastModified'. Use the new keyword if hiding was intended.
        public DateTime? LastModified { get; protected set; }
#pragma warning restore CS0108 // 'NewsSitemapItem.LastModified' hides inherited member 'SitemapItem.LastModified'. Use the new keyword if hiding was intended.

        /// <summary>
        /// How frequently the page is likely to change.
        /// </summary>
#pragma warning disable CS0108 // 'NewsSitemapItem.ChangeFrequency' hides inherited member 'SitemapItem.ChangeFrequency'. Use the new keyword if hiding was intended.
        public SitemapChangeFrequency? ChangeFrequency { get; protected set; }
#pragma warning restore CS0108 // 'NewsSitemapItem.ChangeFrequency' hides inherited member 'SitemapItem.ChangeFrequency'. Use the new keyword if hiding was intended.

        /// <summary>
        /// The priority of this URL relative to other URLs on your site. Valid values range from 0.0 to 1.0.
        /// </summary>
#pragma warning disable CS0108 // 'NewsSitemapItem.Priority' hides inherited member 'SitemapItem.Priority'. Use the new keyword if hiding was intended.
        public double? Priority { get; protected set; }
#pragma warning restore CS0108 // 'NewsSitemapItem.Priority' hides inherited member 'SitemapItem.Priority'. Use the new keyword if hiding was intended.

        public NewsSitemapItem(string url, DateTime? lastModified = null, SitemapChangeFrequency? changeFrequency = null, double? priority = null)
            : base(url, lastModified, changeFrequency, priority)
        {
            Url = url;
            LastModified = lastModified;
            ChangeFrequency = changeFrequency;
            Priority = priority;
        }

        public string PublicationName { get; set; }
        public string PublicationLanguage { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string StockTickers { get; set; }
        public string Access { get; set; }
        public string Genres { get; set; }
        public DateTime? PublicationDate { get; set; }
    }
}