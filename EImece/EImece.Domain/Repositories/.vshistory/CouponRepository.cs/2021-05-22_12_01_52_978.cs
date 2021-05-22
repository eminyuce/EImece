using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
   public class CouponRepository : BaseEntityRepository<Coupon>, ICouponRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CouponRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}
