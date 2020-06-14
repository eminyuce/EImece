using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace EImece.Domain.Helpers.RazorCustomRssTemplate
{
    public class SI : SyndicationItem
    {
        public SI()
        {
            Rank = 0;
        }

        public SI(SyndicationItem syndicationItem)
        {
            Rank = 0;

            SyndicationItem = syndicationItem;

            Title = syndicationItem.Title;
            Summary = syndicationItem.Summary;
            LastUpdatedTime = syndicationItem.LastUpdatedTime;
        }

        public SyndicationItem SyndicationItem { get; set; }

        public string Link
        {
            get
            {
                SyndicationLink firstOrDefault =
                    SyndicationItem.Links.FirstOrDefault(t => t.RelationshipType == "alternate");
                if (firstOrDefault != null)
                {
                    return firstOrDefault.Uri.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Description
        {
            get { return SyndicationItem.Summary.Text; }
        }

        public string ImageLink
        {
            get
            {
                SyndicationLink firstOrDefault =
                    SyndicationItem.Links.FirstOrDefault(t => t.RelationshipType == "enclosure");
                if (firstOrDefault != null)
                {
                    return firstOrDefault.Uri.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public int Rank { get; set; }
        public int Order { get; set; }
        // public DateTime Date { get; set; }

        public string this[String tagName]
        {
            get
            {
                var firstOrDefault = SyndicationItem.ElementExtensions.FirstOrDefault(t => t.OuterName == tagName);
                return firstOrDefault != null ? firstOrDefault.GetObject<string>() : string.Empty;
            }
        }

        public string Descr(int minLen, int maxLen)
        {
            string s = Description != null ? TidyManagedHtmlHelper.StripTags(Description) : "";
            if (s.Length > maxLen) s = s.Substring(0, maxLen).Trim();

            int ix = 0;
            ix = s.LastIndexOf(Environment.NewLine);
            if (ix > minLen)
            {
                s = s.Substring(0, ix).Trim();
            }
            else if ((ix = s.LastIndexOf(".")) > minLen)
            {
                s = s.Substring(0, ix + 1).Trim();
            }
            else if ((ix = s.LastIndexOf(",")) > minLen)
            {
                s = s.Substring(0, ix).Trim();
            }
            else if ((ix = s.LastIndexOf(" ")) > minLen)
            {
                s = s.Substring(0, ix).Trim();
            }

            return s;
        }
    }
}