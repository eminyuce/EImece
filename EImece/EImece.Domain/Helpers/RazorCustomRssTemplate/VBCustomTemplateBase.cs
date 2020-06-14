using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace EImece.Domain.Helpers.RazorCustomRssTemplate
{
    public abstract class VBCustomTemplateBase<T> : TemplateBase<T>
    {
        public string ToUpperCase(string name)
        {
            return name.ToUpper();
        }

        public List<SI> rss(string url, int top = 10, string synKey = "", bool isSubjectSource = true, object featuredFilters = null)
        {
            //TO DO: add filtering
            List<SI> ret = RssHelper.GetRssItems(url);
            ret = ret.Take(top).ToList();

            Dictionary<String, List<String>> featuredItems = AnonymousHelper.AnonymousToDictionary(featuredFilters, true);
            foreach (SI item in ret)
            {
                if (featuredItems.Keys.Any(k => k == "title"))
                {
                    List<string> filters = featuredItems["title"];
                    item.Rank += filters.Count(f => item.Title.Text.ToLower().Contains(f));
                }

                if (featuredItems.Keys.Any(k => k == "link"))
                {
                    List<string> filters = featuredItems["link"];
                    item.Rank += filters.Count(f => item.Link.ToLower().Contains(f));
                }

                if (featuredItems.Keys.Any(k => k == "description"))
                {
                    List<string> filters = featuredItems["description"];
                    item.Rank += filters.Count(f => item.Description.ToLower().Contains(f));
                }
            }

            ret = ret.OrderByDescending(i => i.Rank).ToList();
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i].Order = i;
            }

            if (!string.IsNullOrEmpty(synKey))
            {
                RssInEmail rssInEmail = new RssInEmail() { rssUrl = "url", isSubjectSource = isSubjectSource, items = ret };
                RssHelper.SetRssInEmail(synKey, rssInEmail);
            }

            return ret;
        }

        public string subject(string synKey = "", string defaultSubject = "")
        {
            string ret = defaultSubject;
            if (!string.IsNullOrEmpty(synKey))
            {
                List<RssInEmail> list = RssHelper.GetListRssInEmail(synKey);

                if (list.Any(l => l.isSubjectSource && l.items.Any()))
                {
                    ret = list.Where(l => l.isSubjectSource && l.items.Any()).Select(l => l.items.FirstOrDefault()).FirstOrDefault().Title.Text;
                }
            }

            return ret;
        }

        public string UrlEncode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        public string t(object featuredFilters = null)
        {
            StringBuilder ret = new StringBuilder();

            foreach (System.Reflection.PropertyInfo pi in featuredFilters.GetType().GetProperties())
            {
                ret.Append("name:" + pi.Name);
                ret.Append(" ");
                ret.Append("type:" + pi.PropertyType.ToString());

                ret.Append(" ");
                if (pi.PropertyType == typeof(System.String))
                {
                    ret.Append("value:" + pi.GetValue(featuredFilters, null).ToString());
                }
                else if (pi.PropertyType == typeof(System.String[]))
                {
                    ret.Append("value:" + string.Join("|", (System.String[])pi.GetValue(featuredFilters, null)));
                }
                ret.Append(" ");

                ret.Append(" \n");
            }

            return ret.ToString();
        }

        public string t3(object featuredFilters = null, bool doSplitOnComma = true)
        {
            StringBuilder ret = new StringBuilder();
            Dictionary<String, List<String>> items = AnonymousHelper.AnonymousToDictionary(featuredFilters, doSplitOnComma);

            foreach (string key in items.Keys)
            {
                ret.Append(key);
                ret.Append("\n");
                foreach (string v in items[key])
                {
                    ret.Append(v);
                    ret.Append("\n");
                }

                ret.Append("\n");
            }

            return ret.ToString();
        }

        public string t2()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            MethodBase methodBase = stackFrame.GetMethod();

            StringBuilder ret = new StringBuilder();
            foreach (StackFrame sf in stackTrace.GetFrames())
            {
                ret.Append(sf.GetMethod().Name);
                ret.Append(sf.GetMethod());
                ret.Append("\n");
            }

            return ret.ToString();
        }
    }
}