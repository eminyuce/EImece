using EImece.Domain.Entities;
using Iyzipay.Model;

namespace EImece.Domain.Models.FrontModels
{
    public class PaymentResultViewModel
    {
        public Order Order { get; set; }
        public CheckoutForm CheckoutForm { get; set; }
    }
}