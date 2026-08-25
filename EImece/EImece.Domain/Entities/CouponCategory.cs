using EImece.Domain.GenericRepository;
using System;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class CouponCategory : IEntity<int>
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }
        public int ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
    }
}
