using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ICouponService : IBaseEntityService<Coupon>
    {
        Coupon GetCouponByCode(string code, int lang);

        Task<Coupon> GetCouponByCodeAsync(string code, int lang);

        Task<CouponDto> GetStorefrontCouponByCodeAsync(string code, int lang);

        Task<List<int>> GetCouponProductIdsAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<int>> GetCouponCategoryIdsAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken));

        Task SaveCouponRestrictionsAsync(int couponId, string productIdsCsv, string categoryIdsCsv, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> GetRedemptionCountAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<CouponRedemptionDetailDto>> GetRedemptionsWithDetailsAsync(int couponId, int take, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<CouponRedemption>> GetRecentRedemptionsAsync(int couponId, int take, CancellationToken cancellationToken = default(CancellationToken));
    }
}