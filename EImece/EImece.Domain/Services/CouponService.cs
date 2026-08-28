using EImece.Domain.DependencyInjection;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class CouponService : BaseEntityService<Coupon>, ICouponService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ICouponRepository CouponRepository { get; set; }

        [Inject]
        public ICouponProductRepository CouponProductRepository { get; set; }

        [Inject]
        public ICouponCategoryRepository CouponCategoryRepository { get; set; }

        [Inject]
        public ICouponRedemptionRepository CouponRedemptionRepository { get; set; }

        [Inject]
        public IOrderRepository OrderRepository { get; set; }

        [Inject]
        public ICustomerRepository CustomerRepository { get; set; }

        public CouponService(ICouponRepository repository) : base(repository)
        {
            CouponRepository = repository;
        }

        [Timed("service.coupons.get_by_code_sync")]
        public virtual Coupon GetCouponByCode(string code, int lang)
        {
            return CouponRepository.GetCouponByCode(code, lang);
        }

        [Timed("service.coupons.get_by_code")]
        public virtual async Task<Coupon> GetCouponByCodeAsync(string code, int lang)
        {
            return await CouponRepository.GetCouponByCodeAsync(code, lang).ConfigureAwait(false);
        }

        [Timed("service.coupons.get_storefront_by_code")]
        public virtual async Task<CouponDto> GetStorefrontCouponByCodeAsync(string code, int lang)
        {
            return await CouponRepository.GetStorefrontCouponByCodeAsync(code, lang).ConfigureAwait(false);
        }

        [Timed("service.coupons.get_product_ids")]
        public virtual async Task<List<int>> GetCouponProductIdsAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CouponProductRepository == null) return new List<int>();
            var list = await CouponProductRepository.FindBy(cp => cp.CouponId == couponId).Select(cp => cp.ProductId).ToListAsync(cancellationToken).ConfigureAwait(false);
            return list;
        }

        [Timed("service.coupons.get_category_ids")]
        public virtual async Task<List<int>> GetCouponCategoryIdsAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CouponCategoryRepository == null) return new List<int>();
            var list = await CouponCategoryRepository.FindBy(cc => cc.CouponId == couponId).Select(cc => cc.ProductCategoryId).ToListAsync(cancellationToken).ConfigureAwait(false);
            return list;
        }

        [Timed("service.coupons.save_restrictions")]
        public virtual async Task SaveCouponRestrictionsAsync(int couponId, string productIdsCsv, string categoryIdsCsv, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CouponProductRepository == null || CouponCategoryRepository == null)
            {
                Logger.Warn("CouponProductRepository or CouponCategoryRepository not injected, skipping restriction save");
                return;
            }

            // Clear existing
            var existingProds = await CouponProductRepository.FindBy(cp => cp.CouponId == couponId).ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var ep in existingProds) CouponProductRepository.Delete(ep);
            var existingCats = await CouponCategoryRepository.FindBy(cc => cc.CouponId == couponId).ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var ec in existingCats) CouponCategoryRepository.Delete(ec);
            await CouponProductRepository.SaveAsync().ConfigureAwait(false);
            await CouponCategoryRepository.SaveAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(productIdsCsv))
            {
                var pids = productIdsCsv.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToInt()).Where(i => i > 0).Distinct().ToList();
                foreach (var pid in pids)
                {
                    CouponProductRepository.Add(new CouponProduct { CouponId = couponId, ProductId = pid });
                }
                if (pids.Any()) await CouponProductRepository.SaveAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(categoryIdsCsv))
            {
                var cids = categoryIdsCsv.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToInt()).Where(i => i > 0).Distinct().ToList();
                foreach (var cid in cids)
                {
                    CouponCategoryRepository.Add(new CouponCategory { CouponId = couponId, ProductCategoryId = cid });
                }
                if (cids.Any()) await CouponCategoryRepository.SaveAsync().ConfigureAwait(false);
            }
        }

        [Timed("service.coupons.get_redemption_count")]
        public virtual async Task<int> GetRedemptionCountAsync(int couponId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CouponRedemptionRepository == null) return 0;
            return await CouponRedemptionRepository.FindBy(r => r.CouponId == couponId).CountAsync(cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.coupons.get_recent_redemptions")]
        public virtual async Task<List<CouponRedemption>> GetRecentRedemptionsAsync(int couponId, int take, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CouponRedemptionRepository == null) return new List<CouponRedemption>();
            return await CouponRedemptionRepository.FindBy(r => r.CouponId == couponId).OrderByDescending(r => r.CreatedDate).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.coupons.get_redemptions_with_details")]
        public virtual async Task<List<CouponRedemptionDetailDto>> GetRedemptionsWithDetailsAsync(int couponId, int take, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CouponRedemptionRepository == null) return new List<CouponRedemptionDetailDto>();
            var redemptions = await CouponRedemptionRepository.FindBy(r => r.CouponId == couponId).OrderByDescending(r => r.CreatedDate).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (!redemptions.Any()) return new List<CouponRedemptionDetailDto>();

            var orderIds = redemptions.Select(r => r.OrderId).Distinct().ToList();
            var customerIds = redemptions.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId.Value).Distinct().ToList();

            var orders = new Dictionary<int, string>();
            var customers = new Dictionary<int, string>();

            if (OrderRepository != null && orderIds.Any())
            {
                var orderList = await OrderRepository.FindBy(o => orderIds.Contains(o.Id)).Select(o => new { o.Id, o.OrderNumber }).ToListAsync(cancellationToken).ConfigureAwait(false);
                orders = orderList.ToDictionary(x => x.Id, x => x.OrderNumber);
            }

            if (CustomerRepository != null && customerIds.Any())
            {
                var custList = await CustomerRepository.FindBy(c => customerIds.Contains(c.Id)).Select(c => new { c.Id, Name = c.Name + " " + c.Surname }).ToListAsync(cancellationToken).ConfigureAwait(false);
                customers = custList.ToDictionary(x => x.Id, x => x.Name);
            }

            return redemptions.Select(r => new CouponRedemptionDetailDto
            {
                Id = r.Id,
                CouponCode = r.CouponCode,
                OrderId = r.OrderId,
                OrderNumber = orders.ContainsKey(r.OrderId) ? orders[r.OrderId] : "",
                CustomerId = r.CustomerId,
                CustomerName = r.CustomerId.HasValue && customers.ContainsKey(r.CustomerId.Value) ? customers[r.CustomerId.Value] : "",
                UserId = r.UserId,
                DiscountAmount = r.DiscountAmount,
                CreatedDate = r.CreatedDate
            }).ToList();
        }
    }
}