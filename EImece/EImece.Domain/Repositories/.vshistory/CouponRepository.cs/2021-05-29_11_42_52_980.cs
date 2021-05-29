using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Linq;

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

            var coupons = FindBy(r => r.Lang == lang &&
            r.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)
            && DateTime.Now  > r.StartDate && DateTime.Now <= r.EndDate  )
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate);

            return coupons.FirstOrDefault();
        }
    }
}
