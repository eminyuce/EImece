using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

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

        public Product Product { get; set; }

        [NotMapped]
        public XElement FieldFormat { get; set; }
    }
}