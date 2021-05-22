using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public   string Code { get; set; }
        public string DiscountPercentage { get; set; }
        public string Discount  { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
