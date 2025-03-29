using EImece.Domain.Entities;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICouponRepository : IBaseEntityRepository<Coupon>
    {
        Coupon GetCouponByCode(string code, int lang);
    }
}