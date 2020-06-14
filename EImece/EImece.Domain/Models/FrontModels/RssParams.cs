using System;
using System.Text;

namespace EImece.Domain.Models.FrontModels
{
    public class RssParams
    {
        public int Take { get; set; }
        public int Description { get; set; }
        public int CategoryId { get; set; }
        public int Language { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        //Google tracking
        public string utm_source { get; set; }

        public string utm_medium { get; set; }
        public string utm_campaign { get; set; }

        public string utm_term { get; set; }
        public string utm_content { get; set; }

        //DefaultValues
        public RssParams()
        {
            Take = 10;
            Description = 200;
        }

        public String CustomItemNamespace { get; set; }

        public string GetAnalyticsQueryString()
        {
            StringBuilder ret = new StringBuilder();

            if (!string.IsNullOrEmpty(utm_source))
            {
                ret.Append(string.Format("utm_source={0}", utm_source));
            }

            if (!string.IsNullOrEmpty(utm_medium))
            {
                if (ret.Length > 0) { ret.Append("&"); }
                ret.Append(string.Format("utm_medium={0}", utm_medium));
            }

            if (!string.IsNullOrEmpty(utm_campaign))
            {
                if (ret.Length > 0) { ret.Append("&"); }
                ret.Append(string.Format("utm_campaign={0}", utm_campaign));
            }

            if (!string.IsNullOrEmpty(utm_term))
            {
                if (ret.Length > 0) { ret.Append("&"); }
                ret.Append(string.Format("utm_term={0}", utm_term));
            }

            if (!string.IsNullOrEmpty(utm_content))
            {
                if (ret.Length > 0) { ret.Append("&"); }
                ret.Append(string.Format("utm_content={0}", utm_content));
            }

            //if (ret.Length > 0) { ret.Insert(0, "?"); }

            return ret.ToString();
        }
    }
}