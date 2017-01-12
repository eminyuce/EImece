using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
