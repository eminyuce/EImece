using EImece.Domain.Entities;
using EImece.Domain.Models.Payment;

namespace EImece.Domain.Models.FrontModels
{
    public class PaymentResultViewModel
    {
        public Order Order { get; set; }
        public PaymentResult PaymentResult { get; set; }
    }
}
