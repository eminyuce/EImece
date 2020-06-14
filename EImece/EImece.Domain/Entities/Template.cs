using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class Template : BaseEntity
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TemplateXml))]
        public string TemplateXml { get; set; }
    }
}