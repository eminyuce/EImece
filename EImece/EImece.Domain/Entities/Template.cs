using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class Template : BaseEntity
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TemplateXml))]
        public string TemplateXml { get; set; }
    }
}