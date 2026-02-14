using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class SendMessageToSellerViewModel
    {
        public List<FaqDto> Faqs { get; set; }
        public CustomerDto Customer { get; set; }
    }
}