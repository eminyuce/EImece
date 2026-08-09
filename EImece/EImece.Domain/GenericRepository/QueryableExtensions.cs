using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.GenericRepository
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class QueryableExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static PaginatedList<T> ToPaginatedList<T>(
            this IQueryable<T> query, int pageIndex, int pageSize)
        {
            int totalCount = query.Count();
            IQueryable<T> collection = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PaginatedList<T>(collection, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// EF6 async counterpart of <see cref="ToPaginatedList{T}"/>. Both round trips (the COUNT
        /// and the page itself) are awaited, so the request thread is returned to the pool while
        /// SQL Server does the work instead of blocking on the two queries.
        /// </summary>
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> query, int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            // The page is materialised here rather than handed to PaginatedList as a deferred
            // IQueryable, which is what keeps the second round trip on the async path.
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PaginatedList<T>(items, pageIndex, pageSize, totalCount);
        }
    }
}
