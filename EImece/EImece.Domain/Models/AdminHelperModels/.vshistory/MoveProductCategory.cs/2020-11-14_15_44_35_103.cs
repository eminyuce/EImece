using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.AdminHelperModels
{
    public class MoveProductCategory
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FirstCategoryId))]
        public int FirstCategoryId { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SecondCategoryId))]
        public int SecondCategoryId { get; set; }
    }
}
