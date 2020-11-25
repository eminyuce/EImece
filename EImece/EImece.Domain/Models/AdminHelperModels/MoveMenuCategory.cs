using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.AdminHelperModels
{
    public class MoveMenuCategory
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FirstMenuCategoryId))]
        public int FirstCategoryId { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SecondMenuCategoryId))]
        public int SecondCategoryId { get; set; }
    }
}
