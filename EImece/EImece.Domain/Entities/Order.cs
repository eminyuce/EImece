using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Order : IEntity<int>
    {
        // Entity annotions
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "TestColumnName cannot be longer than 100 characters.")]
        //[Display(Name ="TestColumnName")]
        //[Required(ErrorMessage ="TestColumnName")]
        //[AllowHtml]
        [Key]
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int CustomerId { get; set; }
        public int ShippingAddressId { get; set; }
        public int BillingAddressId { get; set; }
    }
}
