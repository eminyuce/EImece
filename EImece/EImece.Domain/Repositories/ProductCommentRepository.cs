using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;

namespace EImece.Domain.Repositories
{
    public class ProductCommentRepository : BaseEntityRepository<ProductComment>, IProductCommentRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProductCommentRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}