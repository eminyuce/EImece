using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class ListItem : BaseEntity
    {
        [ForeignKey("List")]
        public int ListId { get; set; }
        public string Value { get; set; }

        public  List List { get; set; }
    }
}
