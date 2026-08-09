using EImece.Domain.Entities;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ICouponService : IBaseEntityService<Coupon>
    {
        Coupon GetCouponByCode(string code, int lang);

        Task<Coupon> GetCouponByCodeAsync(string code, int lang);
    }
}