using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class CouponCategoryRepository : BaseRepository<CouponCategory>, ICouponCategoryRepository
    {
        public CouponCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}
