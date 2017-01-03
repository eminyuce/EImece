using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
     public class MenuFile : BaseEntity
    {
        public int MenuId { get; set; }
        public int FileStorageId { get; set; }

        public virtual FileStorage FileStorage { get; set; }
    }
}
