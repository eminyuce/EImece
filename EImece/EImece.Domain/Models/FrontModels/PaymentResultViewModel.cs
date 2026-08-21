using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Payment;

namespace EImece.Domain.Models.FrontModels
{
    public class PaymentResultViewModel
    {
        public OrderDto Order { get; set; }
        public PaymentResult PaymentResult { get; set; }
    }
}
