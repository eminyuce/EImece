using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class ProductSpecificationRepository : BaseEntityRepository<ProductSpecification>, IProductSpecificationRepository
    {
        public ProductSpecificationRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}