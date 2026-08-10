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

        public List<ProductComment> GetAdminPageList(int productId, string search, int lang)
        {
            var productComments = FindBy(r => r.Lang == lang && r.ProductId == productId);
            search = search.ToStr().Trim();
            if (!string.IsNullOrEmpty(search))
            {
                Expression<Func<ProductComment, bool>> whereLamba = r =>
                r.Name.Contains(search) || r.Subject.Contains(search) || r.Review.Contains(search);
                productComments = productComments.Where(whereLamba);
            }
            productComments = productComments.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);
            return productComments.ToList();
        }

        public async Task<List<ProductComment>> GetAdminPageListAsync(int productId, string search, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var productComments = FindBy(r => r.Lang == lang && r.ProductId == productId);
            search = search.ToStr().Trim();
            if (!string.IsNullOrEmpty(search))
            {
                Expression<Func<ProductComment, bool>> whereLamba = r =>
                r.Name.Contains(search) || r.Subject.Contains(search) || r.Review.Contains(search);
                productComments = productComments.Where(whereLamba);
            }
            productComments = productComments.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);
            return await productComments.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}