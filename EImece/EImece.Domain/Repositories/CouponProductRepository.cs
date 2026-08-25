using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class CouponProductRepository : BaseRepository<CouponProduct>, ICouponProductRepository
    {
        public CouponProductRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}
