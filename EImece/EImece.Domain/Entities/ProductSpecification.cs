using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EImece.Domain.Entities
{
    public class ProductSpecification : BaseEntity
    {
        [NotMapped]
        public String GroupName { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [NotMapped]
        public XElement FieldFormat { get; set; }
    }
}
