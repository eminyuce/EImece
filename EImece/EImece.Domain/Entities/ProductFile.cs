using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductFile : BaseEntity
    {
        public int FileStorageId { get; set; }
        public int ProductId { get; set; }


    }
}
