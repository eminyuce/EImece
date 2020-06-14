using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace EImece.Domain.Helpers.Extensions
{
    public static class QuerySortingExtensions
    {
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, IEnumerable<string> sort) where T : class
        {
            if (sort != null)
            {
                List<string> sortFields = new List<string>();

                foreach (string sortField in sort)
                {
                    if (sortField.StartsWith("+"))
                    {
                        sortFields.Add(string.Format("{0} ASC", sortField.TrimStart('+')));
                    }
                    else if (sortField.StartsWith("-"))
                    {
                        sortFields.Add(string.Format("{0} DESC", sortField.TrimStart('-')));
                    }
                    else
                    {
                        sortFields.Add(sortField);
                    }
                }

                return query.OrderBy(string.Join(",", sortFields));
            }

            return query;
        }
    }
}