using EImece.Domain.GenericRepository;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class CouponProduct : IEntity<int>
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
