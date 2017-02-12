using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class MenuFile : BaseEntity
    {
        [ForeignKey("Menu")]
        public int MenuId { get; set; }
        public int FileStorageId { get; set; }

        public  FileStorage FileStorage { get; set; }

        public  Menu Menu { get; set; }
    }
}
