using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum SortingType
    {
        [Description("Default")]
        [Display(Name = "Default")]
        Default = 0,

        [Description("Popularity")]
        [Display(Name = "Popularity")]
        Popularity = 1,

        [Description("LowHighPrice")]
        [Display(Name = "LowHighPrice")]
        LowHighPrice = 2,

        [Description("HighLowPrice")]
        [Display(Name = "HighLowPrice")]
        HighLowPrice = 3,

        [Description("AverageRating")]
        [Display(Name = "AverageRating")]
        AverageRating = 4,

        [Description("AzOrder")]
        [Display(Name = "AzOrder")]
        AzOrder = 5,

        [Description("ZaOrder")]
        [Display(Name = "ZaOrder")]
        ZaOrder = 6,

        [Description("Newest")]
        [Display(Name = "Newest")]
        Newest = 7
    }
}