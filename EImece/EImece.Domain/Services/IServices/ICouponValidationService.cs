using EImece.Domain.Models;
using EImece.Domain.Models.FrontModels;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ICouponValidationService
    {
        Task<CouponValidationResult> ValidateCouponAsync(string couponCode, ShoppingCartSession cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken));

        Task<CouponValidationResult> ValidateCouponAsync(string couponCode, BuyWithNoAccountCreation cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken));

        // Revalidation helper for cart already containing coupon
        Task<CouponValidationResult> RevalidateActiveCouponAsync(ShoppingCartSession cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken));
    }
}
