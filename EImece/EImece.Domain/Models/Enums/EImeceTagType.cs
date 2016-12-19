using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.Enums
{
    public enum EImeceTagType
    {
        [Display(Name = "Products")]
        Products = 1,
        [Display(Name = "Stories")]
        Stories = 2
    }
}
