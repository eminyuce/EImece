using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrderDetailViewModel
    {
        public OrderDto Order { get; set; }
        public CustomerSummaryDto Customer { get; set; }
    }
}