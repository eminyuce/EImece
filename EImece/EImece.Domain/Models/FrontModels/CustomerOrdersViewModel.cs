using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrdersViewModel : ItemListing
    {
        public List<Order> Orders { get; set; }
        public Customer Customer { get; set; }
    }
}