using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;

namespace EImece.Domain.Services
{
    public class CouponService : BaseEntityService<Coupon>, ICouponService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ICouponRepository CouponRepository { get; set; }

        public CouponService(ICouponRepository repository) : base(repository)
        {
            CouponRepository = repository;
        }

        public Coupon GetCouponByCode(string code, int lang)
        {
            return CouponRepository.GetCouponByCode(code, lang);
        }
    }
}