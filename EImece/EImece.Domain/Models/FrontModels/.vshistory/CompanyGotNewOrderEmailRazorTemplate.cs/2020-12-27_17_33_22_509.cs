using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class CompanyGotNewOrderEmailRazorTemplate : RazorTemplateModel
    {
        public string EmailSubject { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string CompanyEmailAddress { get; set; }
        public string CompanyWebSiteUrl { get; set; }
        public string ImgLogoSrc { get; set; }
        public Order FinishedOrder { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }
    }
}
