using EImece.Domain.Entities;
using Newtonsoft.Json;
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
        public ProductDetailViewModel ProductDetailViewModel { get; set; }
        public Address ShippingAddress { get; set; }

        [JsonIgnore]
        public decimal CargoPriceValue
        {
            get
            {
                if (TotalPrice == 0)
                    return 0;
                else if (BasketMinTotalPriceForCargoInt > 0 && TotalPrice > BasketMinTotalPriceForCargoInt)
                    return 0;
                else if (CargoPrice.SettingValue.ToDecimal() > 0)
                    return CargoPrice.SettingValue.ToDecimal();
                else
                    return 0;
            }
        }
    }
}
