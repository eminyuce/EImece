using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ProductFile : BaseEntity
    {
        public int FileStorageId { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public  FileStorage FileStorage { get; set; }
        public  Product Product { get; set; }

    }
}
