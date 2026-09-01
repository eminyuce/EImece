namespace EImece.Domain.Models.Enums
{
    public enum CouponValidationReason
    {
        None = 0,
        Valid = 1,
        CouponNotFound = 2,
        CouponInactive = 3,
        CouponExpired = 4,
        CouponNotYetValid = 5,
        MinOrderAmountNotMet = 6,
        NotApplicableToCartItems = 7,
        UsageLimitReached = 8,
        CustomerUsageLimitReached = 9,
        AlreadyUsedByCustomer = 10,
        FirstOrderOnly = 11,
        BirthdayNotEligible = 12,
        LoginRequired = 13,
        StackingNotAllowed = 14,
        SaleItemsExcluded = 15,
        InvalidCurrency = 16,
        InvalidDiscount = 17,
        AssignedToOtherCustomer = 18
    }
}
