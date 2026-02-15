using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrdersViewModel : ItemListing
    {
        public List<OrderDto> Orders { get; set; }
        public CustomerDto Customer { get; set; }
    }
}