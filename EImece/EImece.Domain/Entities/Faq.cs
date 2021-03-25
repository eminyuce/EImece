using Resources;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Faq : BaseEntity
    {
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [StringLength(500, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Question))]
        public string Question { get; set; }

        [AllowHtml]
        public string Answer { get; set; }

        public string AddUserId { get; set; }
        public string UpdateUserId { get; set; }
    }
}