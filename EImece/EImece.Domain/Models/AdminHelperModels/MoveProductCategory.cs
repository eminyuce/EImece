using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.AdminHelperModels
{
    public class MoveProductCategory
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstCategoryId))]
        public int FirstCategoryId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SecondCategoryId))]
        public int SecondCategoryId { get; set; }
    }
}