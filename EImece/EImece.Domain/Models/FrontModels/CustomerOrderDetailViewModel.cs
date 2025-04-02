using EImece.Domain.Entities;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrderDetailViewModel
    {
        public Order Order { get; set; }
        public Customer Customer { get; set; }
    }
}