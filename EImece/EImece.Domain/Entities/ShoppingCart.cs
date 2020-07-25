using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class ShoppingCart : BaseEntity 
    {
        public string OrderGuid { get; set; }
        public string ShoppingCartJson { get; set; }

    }
}
