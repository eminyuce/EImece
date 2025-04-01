using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum ProductState
    {
        [Display(Name = "NONE")]
        NONE = 0,

        [Display(Name = "Product In Stock")]
        ProductInStock = 1,

        [Display(Name = "Product Out of Stock")]
        ProductOutOfStock = 2,

        [Display(Name = "Pre-Order Available")]
        PreOrder = 3,

        [Display(Name = "Discontinued")]
        Discontinued = 4,

        [Display(Name = "Backorder Available")]
        Backorder = 5,

        [Display(Name = "Coming Soon")]
        ComingSoon = 6,

        [Display(Name = "Limited Stock Available")]
        LimitedStock = 7,

        [Display(Name = "Reserved for Customers")]
        Reserved = 8,

        [Display(Name = "Awaiting Restock")]
        AwaitingRestock = 9,

        [Display(Name = "Not for Sale")]
        NotForSale = 10
    }
}
