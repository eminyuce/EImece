using System;
using System.Collections.Generic;

namespace EImece.Domain.Entities
{
    public class List : BaseEntity
    {
        public Boolean IsService { get; set; }
        public Boolean IsValues { get; set; }

        public ICollection<ListItem> ListItems { get; set; }
    }
}