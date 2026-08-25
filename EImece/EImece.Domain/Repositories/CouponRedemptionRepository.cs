using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class CouponRedemptionRepository : BaseEntityRepository<CouponRedemption>, ICouponRedemptionRepository
    {
        public CouponRedemptionRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public virtual async Task<int> GetGlobalRedemptionCountAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FindBy(r => r.CouponId == couponId).CountAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<int> GetCustomerRedemptionCountAsync(int couponId, string userId, int? customerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = FindBy(r => r.CouponId == couponId);
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(r => r.UserId == userId);
            else if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);
            else
                return 0;

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> HasCustomerEverUsedCouponAsync(int couponId, string userId, int? customerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = FindBy(r => r.CouponId == couponId);
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(r => r.UserId == userId);
            else if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);
            else
                return false;

            return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
