using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class CouponService : BaseEntityService<Coupon>, ICouponService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    }
}
