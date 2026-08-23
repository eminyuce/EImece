using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EImece.Domain.Entities
{
    public class ProductSpecification : BaseEntity
    {
        // Needed for Admin panel — admin product spec grid groups specs by GroupName without a DB column.
        [NotMapped]
        public string GroupName { get; set; }

        [AllowHtml]
        public string Value { get; set; }

        public string Unit { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        // Needed for Admin panel — admin template editor holds the parsed XML field format transiently.
        [NotMapped]
        public XElement FieldFormat { get; set; }
    }
}