using EImece.Domain.Entities;

namespace EImece.Domain.Services.IServices
{
    public interface ICouponService : IBaseEntityService<Coupon>
    {
        Coupon GetCouponByCode(string code, int lang);
    }
}