using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum EImeceOrderStatus
    {
        [Description("Yeni Sipariş")]
        [Display(Name = "Türkçe")]
        NewlyOrder = 1,

        [Description("Yeni Sipariş")]
        [Display(Name = "Devam etmekte")]
        InProgress = 2,

        [Description("Yeni Sipariş")]
        [Display(Name = "İptal edildi")]
        Cancelled = 3,

        [Description("Yeni Sipariş")]
        [Display(Name = "İade")]
        Returned = 4,

        [Description("Yeni Sipariş")]
        [Display(Name = "Bitmiş Sipariş")]
        Delivered = 5
    }
}
