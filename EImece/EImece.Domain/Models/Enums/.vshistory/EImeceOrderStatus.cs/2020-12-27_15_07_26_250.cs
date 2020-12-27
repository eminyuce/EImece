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
        [Display(Name = "Yeni Sipariş")]
        NewlyOrder = 1,
        [Display(Name = "Devam etmekte")]
        InProgress = 2,
        [Display(Name = "İptal edildi")]
        Cancelled = 3,
        [Display(Name = "İade")]
        Returned = 4,
        [Display(Name = "Bitmiş Sipariş")]
        FinishedOrder = 5
    }
}
