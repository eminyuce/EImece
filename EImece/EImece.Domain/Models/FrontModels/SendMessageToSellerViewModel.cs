using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class SendMessageToSellerViewModel
    {
        public List<Faq> Faqs { get; set; }
        public Customer Customer { get; set; }
    }
}