using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductTag : IEntity<int>
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public int ProductId { get; set; }

        public virtual Tag Tag { get; set; }

    }
}
