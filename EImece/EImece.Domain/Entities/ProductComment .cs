using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductComment :BaseEntity
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Review { get; set; }
        public string Email { get; set; }  
        public string Subject { get; set; }
        public int Rating { get; set; }

        [NotMapped]
        public string SeoUrl { get; set; }
    }
}
