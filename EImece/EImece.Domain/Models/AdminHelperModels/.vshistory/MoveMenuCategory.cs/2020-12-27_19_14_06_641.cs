using Resources;
using System.ComponentModel.DataAnnotations;

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