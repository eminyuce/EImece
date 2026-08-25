using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum CouponDiscountType
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.DiscountTypeFixedAmount))]
        FixedAmount = 0,

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.DiscountTypePercentage))]
        Percentage = 1,

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.DiscountTypeFreeShipping))]
        FreeShipping = 2
    }
}
