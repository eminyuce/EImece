using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.HelperModels
{
    public class OrderingItem
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public bool IsActive { get; set; }
    }
}
