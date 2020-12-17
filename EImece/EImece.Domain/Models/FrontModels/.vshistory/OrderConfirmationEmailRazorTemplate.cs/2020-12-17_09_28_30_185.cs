using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class OrderConfirmationEmailRazorTemplate
    {
        public string CompanyAddress { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string CompanyEmailAddress { get; set; }
        public string OrderNumber { get; set; }
        public string OrderDate{ get; set; }  
        public List<Order> Orders { get; set; }
        public Customer Customer { get; set; }
    }
}
