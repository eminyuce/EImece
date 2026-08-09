using EImece.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICouponRepository : IBaseEntityRepository<Coupon>
    {
        Coupon GetCouponByCode(string code, int lang);

        Task<Coupon> GetCouponByCodeAsync(string code, int lang, CancellationToken cancellationToken = default(CancellationToken));
    }
}