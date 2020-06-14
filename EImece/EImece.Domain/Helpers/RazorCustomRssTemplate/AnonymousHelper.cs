using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers.RazorCustomRssTemplate
{
    public class AnonymousHelper
    {
        public static Dictionary<String, List<String>> AnonymousToDictionary(object anonymousObject = null, bool doSplitOnComma = false)
        {
            Dictionary<String, List<String>> items = new Dictionary<String, List<String>>();

            if (anonymousObject != null && anonymousObject.GetType().GetProperties().Any())
            {
                foreach (System.Reflection.PropertyInfo pi in anonymousObject.GetType().GetProperties())
                {
                    string key = pi.Name.ToLower();
                    List<String> list = new List<string>();

                    if (pi.PropertyType == typeof(System.String))
                    {
                        string v = (System.String)pi.GetValue(anonymousObject, null);
                        if (doSplitOnComma)
                        {
                            list.AddRange(SplitOnComma(v).Select(s => s.ToLower()));
                        }
                        else
                        {
                            list.Add(v.ToLower());
                        }
                    }
                    else if (pi.PropertyType == typeof(System.String[]))
                    {
                        String[] v = (System.String[])pi.GetValue(anonymousObject, null);
                        list.AddRange(v.ToList().Select(s => s.ToLower()));
                    }

                    if (items.Keys.Contains(key))
                    {
                        items[key].AddRange(list);
                    }
                    else
                    {
                        items.Add(key, list);
                    }
                }
            }

            return items;
        }

        private static List<String> SplitOnComma(string value)
        {
            return value.Split(',').Select(s => s.Trim()).ToList();
        }
    }
}