using EImece.Domain.Models.DTOs;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrderDetailViewModel
    {
        public OrderDto Order { get; set; }
        public CustomerDto Customer { get; set; }
    }
}