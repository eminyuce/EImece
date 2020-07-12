using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ShoppingCartItem
    {
        public int quantity { set; get; }
        public Product product { set; get; }
    }
}
