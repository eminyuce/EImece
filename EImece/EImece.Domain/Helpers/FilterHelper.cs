using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Helpers
{
    public class FilterHelper
    {
        public static List<Filter> ParseFiltersFromString(string filters)
        {
            var items = new List<Filter>();

            if (!string.IsNullOrEmpty(filters))
            {
                var stringFilters = filters.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var stringFilter in stringFilters)
                {
                    if (stringFilter.IndexOf('-') > 0)
                    {
                        var filter = new Filter();
                        var firstHyphen = stringFilter.IndexOf('-');
                        filter.FieldName = stringFilter.Substring(0, firstHyphen).UrlDecode();

                        if (stringFilter.IndexOf('-', firstHyphen + 1) > 0)
                        {
                            var secondHyphen = stringFilter.IndexOf('-', firstHyphen + 1);
                            filter.ValueFirst =
                                stringFilter.Substring(firstHyphen + 1, secondHyphen - firstHyphen - 1).UrlDecode();
                            filter.ValueLast =
                                stringFilter.Substring(secondHyphen + 1, stringFilter.Length - secondHyphen - 1).
                                    UrlDecode();
                        }
                        else
                        {
                            filter.ValueFirst =
                                stringFilter.Substring(firstHyphen + 1, stringFilter.Length - firstHyphen - 1).UrlDecode();
                        }

                        items.Add(filter);
                    }
                }
            }
            return items;
        }
    }
}