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

        [Description("Hazırlanmakta Olan Sipariş")]
        [Display(Name = "InProgress")]
        InProgress = 2,

        [Description("İptal edilmis Sipariş")]
        [Display(Name = "Cancelled")]
        Cancelled = 3,

        [Description("İade Sipariş")]
        [Display(Name = "Returned")]
        Returned = 4,

        [Description("Bitmiş Sipariş")]
        [Display(Name = "Delivered")]
        Delivered = 5
    }
}
