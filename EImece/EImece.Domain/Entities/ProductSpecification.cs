using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Resources;

namespace EImece.Domain.Entities
{
    public class ProductSpecification : BaseEntity
    {
        [NotMapped]
        public string GroupName { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public  Product Product { get; set; }

        [NotMapped]
        public XElement FieldFormat { get; set; }
    }
}
