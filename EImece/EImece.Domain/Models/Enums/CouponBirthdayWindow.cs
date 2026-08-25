using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum CouponBirthdayWindow
    {
        [Display(Name = "Week")]
        Week = 0,

        [Display(Name = "Month")]
        Month = 1
    }
}
