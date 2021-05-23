using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; }
        public int DiscountPercentage { get; set; }
        public int Discount  { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
