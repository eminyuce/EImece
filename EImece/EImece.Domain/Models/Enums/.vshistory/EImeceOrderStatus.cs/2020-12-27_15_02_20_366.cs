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
        [Display(Name = "NewlyOrder")]
        NewlyOrder = 1,
        [Display(Name = "InProgress")]
        InProgress = 2,
        [Display(Name = "Cancelled")]
        Cancelled = 2,
        [Display(Name = "FinishedOrder")]
        FinishedOrder = 3
    }
}
