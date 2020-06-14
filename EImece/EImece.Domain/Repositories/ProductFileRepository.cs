using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class ProductFileRepository : BaseEntityRepository<ProductFile>, IProductFileRepository
    {
        public ProductFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}