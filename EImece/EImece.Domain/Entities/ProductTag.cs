using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductTag : IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Tag")]
        public int TagId { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public virtual Tag Tag { get; set; }
        public virtual Product Product { get; set; }

    }
}
