﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class OrderConfirmationEmailRazorTemplate
    {
        public string CompanyAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyEmailAddress { get; set; }
        public string OrderNumber { get; set; }
        public string OrderDate{ get; set; }  
        public string DeliveryAddress { get; set; }
    }
}
