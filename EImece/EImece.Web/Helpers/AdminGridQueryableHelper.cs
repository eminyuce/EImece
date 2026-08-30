using EImece.Domain.Entities;
using System;
using System.Linq;
using System.Web;

namespace EImece.Web.Helpers
{
    /// <summary>
    /// Griddly QueryableResult applies OrderBy Id ASC when sortFields are absent (gridembed=1 first load).
    /// DefaultSort from GriddlySettings is only applied for child actions, not embed requests.
    /// </summary>
    public static class AdminGridQueryableHelper
    {
        public static bool HasGriddlySortFields(HttpRequestBase request)
        {
            if (request?.Params == null)
            {
                return false;
            }

            foreach (string key in request.Params.AllKeys)
            {
                if (key != null && key.StartsWith("sortFields[", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static IQueryable<T> EnsureUpdatedDateSort<T>(IQueryable<T> query, HttpRequestBase request)
            where T : BaseEntity
        {
            if (HasGriddlySortFields(request))
            {
                return query;
            }

            return query.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.Id);
        }
    }
}
