using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class ProductCommentRepository : BaseEntityRepository<ProductComment>, IProductCommentRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProductCommentRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<ProductComment> GetAdminPageList(int? productId, string search, int lang, IList<int> ratings = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            return BuildAdminQuery(productId, search, lang, ratings, startDate, endDate).ToList();
        }

        public async Task<List<ProductComment>> GetAdminPageListAsync(int? productId, string search, int lang, IList<int> ratings = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await BuildAdminQuery(productId, search, lang, ratings, startDate, endDate).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        private IQueryable<ProductComment> BuildAdminQuery(int? productId, string search, int lang, IList<int> ratings, DateTime? startDate, DateTime? endDate)
        {
            Expression<Func<ProductComment, object>> includeProduct = r => r.Product;
            var comments = GetAllIncluding(includeProduct);
            return ProductCommentAdminListHelper.ApplyAdminFilters(comments, lang, productId, search, ratings, startDate, endDate);
        }
    }
}
