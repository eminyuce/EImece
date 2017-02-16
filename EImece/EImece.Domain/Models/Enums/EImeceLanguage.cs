using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.Enums
{
    public enum EImeceLanguage
    {
        [Description("tr-TR")]
        [Display(Name = "Turkish")]
        Turkish =1,
        [Description("en-US")]
        [Display(Name = "English")]
        English =2
    }
}
