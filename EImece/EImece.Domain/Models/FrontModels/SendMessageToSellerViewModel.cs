using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class SendMessageToSellerViewModel
    {
        public List<Faq> Faqs { get; set; }
        public Customer Customer { get; set; }
    }
}
