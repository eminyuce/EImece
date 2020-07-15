using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class ShoppingCart :  BaseEntity
    {
        // Entity annotions
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "TestColumnName cannot be longer than 100 characters.")]
        //[Display(Name ="TestColumnName")]
        //[Required(ErrorMessage ="TestColumnName")]
        //[AllowHtml]
        public int CustomerId { get; set; }
        public string Locale { get; set; }
        public string ConversationId { get; set; }
        public string Price { get; set; }
        public string PaidPrice { get; set; }
        public string Currency { get; set; }
        public string BasketId { get; set; }
        public string PaymentGroup { get; set; }

        public int ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; }
        public int BillingAddressId { get; set; }
        public Address BillingAddress { get; set; }
    }
}
