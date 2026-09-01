using EImece.Domain.Caching;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers.RazorCustomRssTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace EImece.Domain.Helpers
{
    public class RssHelper
    {
        public static List<RssInEmail> GetListRssInEmail(string synKey)
        {
            return GetListRssInEmail(synKey, null);
        }

        public static List<RssInEmail> GetListRssInEmail(string synKey, IEimeceCacheProvider cache)
        {
            var ret = new List<RssInEmail>();
            cache = ResolveCache(cache);
            if (cache == null || string.IsNullOrEmpty(synKey))
            {
                return ret;
            }

            List<RssInEmail> cached;
            if (cache.Get(CacheKeys.RssEmail(synKey), out cached) && cached != null)
            {
                ret = cached;
            }

            return ret;
        }

        public static void SetRssInEmail(string synKey, RssInEmail rssInEmail)
        {
            SetRssInEmail(synKey, rssInEmail, null);
        }

        public static void SetRssInEmail(string synKey, RssInEmail rssInEmail, IEimeceCacheProvider cache)
        {
            if (rssInEmail == null || string.IsNullOrEmpty(synKey))
            {
                return;
            }

            cache = ResolveCache(cache);
            if (cache == null)
            {
                return;
            }

            var key = CacheKeys.RssEmail(synKey);
            List<RssInEmail> list;
            if (!cache.Get(key, out list) || list == null)
            {
                list = new List<RssInEmail>();
            }

            list.Add(rssInEmail);
            cache.Set(key, list, CachePolicy.Sliding(60));
        }

        public static List<SI> GetRssItems(string url)
        {
            List<SI> result = new List<SI>();

            try
            {
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                result = feed.Items.ToList().ConvertAll(SyndicationItemToSI);
                reader.Close();
            }
            catch
            {
            }

            return result;
        }

        public static SI SyndicationItemToSI(SyndicationItem syndicationItem)
        {
            return new SI(syndicationItem);
        }

        public static SyndicationFeed GetRssFeedCached(String url)
        {
            return GetRssFeedCached(url, null);
        }

        public static SyndicationFeed GetRssFeedCached(String url, IEimeceCacheProvider cache)
        {
            cache = ResolveCache(cache);
            if (cache == null)
            {
                return GetRss(url);
            }

            return cache.GetOrAdd(CacheKeys.RssFeed(url), () => GetRss(url), CachePolicy.Absolute(600));
        }

        public static SyndicationFeed GetRss(String url)
        {
            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            return feed;
        }

        public static string GetRssItemValue(SyndicationItem rssItem, string elementName)
        {
            var extentionElement = rssItem.ElementExtensions.FirstOrDefault(ee => ee.OuterName.ToLower() == elementName.ToLower());
            if (extentionElement != null)
            {
                return extentionElement.GetObject<XElement>().Value;
            }
            return String.Empty;
        }

        private static IEimeceCacheProvider ResolveCache(IEimeceCacheProvider cache)
        {
            return cache ?? DomainServiceProvider.GetService<IEimeceCacheProvider>();
        }
    }
}
