using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Resources;

namespace EImece.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Note))]
        public string Note { get; set; }
    }
}
