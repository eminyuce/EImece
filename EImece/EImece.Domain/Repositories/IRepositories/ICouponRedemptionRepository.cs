using EImece.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICouponRedemptionRepository : IBaseEntityRepository<CouponRedemption>
    {
        Task<int> GetGlobalRedemptionCountAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> GetCustomerRedemptionCountAsync(int couponId, string userId, int? customerId, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> HasCustomerEverUsedCouponAsync(int couponId, string userId, int? customerId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
