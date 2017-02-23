using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericRepository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Resources;

namespace EImece.Domain.Entities
{
    public class List : BaseEntity
    {
        public Boolean IsService { get; set; }
        public Boolean IsValues { get; set; }

        public  ICollection<ListItem> ListItems { get; set; }
    }
}
