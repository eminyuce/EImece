using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class CustomerOrderDetailViewModel
    {
        public Order Order { get; set; }
        public Customer Customer { get; set; }
    }
}
