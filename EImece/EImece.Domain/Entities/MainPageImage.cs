using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class MainPageImage : BaseContent
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Link))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        public string Link { get; set; }
    }
}