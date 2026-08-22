using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrdersViewModel : ItemListing
    {
        public List<OrderListItemDto> Orders { get; set; }
        public CustomerSummaryDto Customer { get; set; }

        public CustomerOrdersViewModel()
        {
            Orders = new List<OrderListItemDto>();
        }
    }
}