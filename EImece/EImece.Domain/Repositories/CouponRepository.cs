using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class CouponRepository : BaseEntityRepository<Coupon>, ICouponRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CouponRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public Coupon GetCouponByCode(string code, int lang)
        {
            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Coupon.Code cannot be empty or null");
            }

            var coupons = FindBy(r => r.Lang == lang && r.IsActive &&
            r.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)
            && DateTime.Now > r.StartDate && DateTime.Now <= r.EndDate)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate);

            return coupons.FirstOrDefault();
        }

        public async Task<Coupon> GetCouponByCodeAsync(string code, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Coupon.Code cannot be empty or null");
            }

            var coupons = FindBy(r => r.Lang == lang && r.IsActive &&
            r.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)
            && DateTime.Now > r.StartDate && DateTime.Now <= r.EndDate)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate);

            return await coupons.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Storefront cart coupon read: projects only Code, Name, Discount, DiscountPercentage — the
        /// fields the cart session and checkout views consume. AsNoTracking, no AutoMapper hop.
        /// </summary>
        public async Task<CouponDto> GetStorefrontCouponByCodeAsync(string code, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Coupon.Code cannot be empty or null");
            }

            return await EImeceDbContext.Coupons.AsNoTracking()
                .Where(r => r.Lang == lang && r.IsActive &&
                    r.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase) &&
                    DateTime.Now > r.StartDate && DateTime.Now <= r.EndDate)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(r => new CouponDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    Name = r.Name,
                    Discount = r.Discount,
                    DiscountPercentage = r.DiscountPercentage,
                    AssignedUserId = r.AssignedUserId,
                    AssignedCustomerId = r.AssignedCustomerId,
                    DiscountType = r.DiscountType,
                    MaximumDiscountAmount = r.MaximumDiscountAmount,
                    GlobalUsageLimit = r.GlobalUsageLimit,
                    PerCustomerUsageLimit = r.PerCustomerUsageLimit,
                    MinimumOrderAmount = r.MinimumOrderAmount,
                    ExcludeSaleItems = r.ExcludeSaleItems,
                    IsFreeShipping = r.IsFreeShipping,
                    AllowStacking = r.AllowStacking,
                    RequireLogin = r.RequireLogin,
                    IsFirstOrderOnly = r.IsFirstOrderOnly,
                    IsNewCustomerOnly = r.IsNewCustomerOnly,
                    IsBirthdayCoupon = r.IsBirthdayCoupon,
                    BirthdayWindow = r.BirthdayWindow,
                    Currency = r.Currency
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}