using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class MainPageImage : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Link))]
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        public string Link { get; set; }
    }
}