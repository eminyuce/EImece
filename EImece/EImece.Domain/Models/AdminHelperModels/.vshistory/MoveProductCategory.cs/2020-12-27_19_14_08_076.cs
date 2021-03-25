using Resources;
using System.ComponentModel.DataAnnotations;

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