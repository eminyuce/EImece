using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using Iyzipay.Model;

namespace EImece.Domain.Models.FrontModels
{
    public class PaymentResultViewModel
    {
        public Order Order { get; set; }
        public  CheckoutForm CheckoutForm { get; set; }
    }
}
