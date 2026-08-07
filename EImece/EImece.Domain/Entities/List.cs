using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class List : BaseEntity
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsService))]
        public Boolean IsService { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsValues))]
        public Boolean IsValues { get; set; }

        public ICollection<ListItem> ListItems { get; set; }
    }
}
