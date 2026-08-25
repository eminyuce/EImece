using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum CouponBirthdayWindow
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.BirthdayWindowWeek))]
        Week = 0,

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.BirthdayWindowMonth))]
        Month = 1
    }
}
