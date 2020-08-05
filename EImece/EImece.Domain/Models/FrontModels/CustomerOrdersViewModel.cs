using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrdersViewModel:ItemListing
    {
        public List<Order> Orders { get; set; }
        public Customer Customer { get; set; }
    }
}
