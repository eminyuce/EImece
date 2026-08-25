using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum CouponDiscountType
    {
        [Display(Name = "FixedAmount")]
        FixedAmount = 0,

        [Display(Name = "Percentage")]
        Percentage = 1,

        [Display(Name = "FreeShipping")]
        FreeShipping = 2
    }
}
