using EImece.Domain.Helpers.RazorCustomRssTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace EImece.Domain.Helpers
{
    public class RssHelper
    {
        public static List<RssInEmail> GetListRssInEmail(string synKey)
        {
            string key = synKey;
            List<RssInEmail> ret = new List<RssInEmail>();
            ObjectCache cache = MemoryCache.Default;

            CacheItem ci = cache.GetCacheItem(key);

            if (ci != null)
            {
                ret = (List<RssInEmail>)ci.Value;
            }

            return ret;
        }

        public static void SetRssInEmail(string synKey, RssInEmail rssInEmail)
        {
            List<RssInEmail> list = new List<RssInEmail>();
            string key = synKey;

            ObjectCache cache = MemoryCache.Default;

            CacheItem ci = cache.GetCacheItem(key);

            if (ci != null)
            {
                list = (List<RssInEmail>)ci.Value;
                list.Add(rssInEmail);
            }
            else
            {
                list.Add(rssInEmail);
                ci = new CacheItem(key, list);
            }

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = new TimeSpan(0, 1, 0);

            cache.Set(ci, policy);
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
            string key = url;
            var products = (SyndicationFeed)MemoryCache.Default.Get(key);
            if (products == null)
            {
                products = GetRss(url);
                CacheItemPolicy policy = null;

                policy = new CacheItemPolicy();
                policy.Priority = CacheItemPriority.Default;
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(600);

                MemoryCache.Default.Set(key, products, policy);
            }
            return products;
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
    }
}