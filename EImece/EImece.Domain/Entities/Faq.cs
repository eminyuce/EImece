using Resources;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Faq : BaseEntity
    {
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [StringLength(500, ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Question))]
        public string Question { get; set; }

        [AllowHtml]
        public string Answer { get; set; }

        public string AddUserId { get; set; }
        public string UpdateUserId { get; set; }
    }
}