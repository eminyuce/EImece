using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace EImece.Domain.Entities
{
    public class Template : BaseEntity
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TemplateXml))]
        public string TemplateXml { get; set; }
    }
}
