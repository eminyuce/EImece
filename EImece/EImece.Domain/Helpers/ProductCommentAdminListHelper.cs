using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public static class ProductCommentAdminListHelper
    {
        public static IQueryable<ProductComment> ApplyAdminFilters(
            IQueryable<ProductComment> comments,
            int lang,
            int? productId,
            string search,
            IList<int> ratings,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (comments == null)
            {
                return Enumerable.Empty<ProductComment>().AsQueryable();
            }

            var query = comments.Where(r => r.Lang == lang);

            if (productId.HasValue && productId.Value > 0)
            {
                var pid = productId.Value;
                query = query.Where(r => r.ProductId == pid);
            }

            var stars = NormalizeRatings(ratings);
            if (stars.Count > 0)
            {
                query = query.Where(r => stars.Contains(r.Rating));
            }

            DateTime? fromDate;
            DateTime? toDate;
            NormalizeDateRange(startDate, endDate, out fromDate, out toDate);

            if (fromDate.HasValue)
            {
                var from = fromDate.Value;
                query = query.Where(r => r.UpdatedDate >= from);
            }

            if (toDate.HasValue)
            {
                var toExclusive = toDate.Value;
                query = query.Where(r => r.UpdatedDate < toExclusive);
            }

            search = search.ToStr().Trim();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    (r.Name != null && r.Name.Contains(search))
                    || (r.Subject != null && r.Subject.Contains(search))
                    || (r.Review != null && r.Review.Contains(search))
                    || (r.Email != null && r.Email.Contains(search))
                    || (r.UserId != null && r.UserId.Contains(search))
                    || (r.Product != null && r.Product.Name != null && r.Product.Name.Contains(search))
                    || (r.Product != null && r.Product.NameShort != null && r.Product.NameShort.Contains(search))
                    || (r.Product != null && r.Product.ProductCode != null && r.Product.ProductCode.Contains(search)));
            }

            return query.OrderByDescending(r => r.UpdatedDate);
        }

        public static List<int> ParseRatings(IEnumerable<int> ratingValues, string ratingsCsv)
        {
            var values = new List<int>();
            if (ratingValues != null)
            {
                values.AddRange(ratingValues);
            }

            if (!string.IsNullOrWhiteSpace(ratingsCsv))
            {
                var parts = ratingsCsv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < parts.Length; i++)
                {
                    int parsed;
                    if (int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    {
                        values.Add(parsed);
                    }
                }
            }

            return NormalizeRatings(values);
        }

        public static List<int> NormalizeRatings(IEnumerable<int> ratings)
        {
            if (ratings == null)
            {
                return new List<int>();
            }

            return ratings.Where(r => r >= 1 && r <= 5).Distinct().OrderBy(r => r).ToList();
        }

        public static void NormalizeDateRange(DateTime? startDate, DateTime? endDate, out DateTime? fromDate, out DateTime? toExclusive)
        {
            DateTime? start = startDate.HasValue ? startDate.Value.Date : (DateTime?)null;
            DateTime? end = endDate.HasValue ? endDate.Value.Date : (DateTime?)null;

            if (start.HasValue && end.HasValue && start.Value > end.Value)
            {
                var swap = start;
                start = end;
                end = swap;
            }

            fromDate = start;
            toExclusive = end.HasValue ? end.Value.AddDays(1) : (DateTime?)null;
        }
    }
}
