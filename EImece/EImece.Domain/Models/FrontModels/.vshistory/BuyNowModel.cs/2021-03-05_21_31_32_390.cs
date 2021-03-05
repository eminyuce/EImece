using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class BuyNowModel
    {
        public int ProductId { get; set; }
        public Customer Customer { get; set; }
        public string OrderGuid { get; set; }
        public string ConversationId { get; set; }
        public ProductDetailViewModel Product { get; set; }

        private Address ShippingAddress { get; set; }
    }
}
