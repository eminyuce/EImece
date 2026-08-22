using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class SendMessageToSellerViewModel
    {
        public List<FaqSummaryDto> Faqs { get; set; }
        public CustomerSummaryDto Customer { get; set; }
    }
}