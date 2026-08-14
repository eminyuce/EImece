using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrderDetailViewModel
    {
        public OrderDto Order { get; set; }
        public Customer Customer { get; set; }
    }
}