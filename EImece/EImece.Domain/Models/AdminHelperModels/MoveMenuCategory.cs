using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.AdminHelperModels
{
    public class MoveMenuCategory
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstMenuCategoryId))]
        public int FirstCategoryId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SecondMenuCategoryId))]
        public int SecondCategoryId { get; set; }
    }
}