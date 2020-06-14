using System.Collections.Generic;

namespace EImece.Domain.Entities
{
    public class TagCategory : BaseEntity
    {
        public ICollection<Tag> Tags { get; set; }
    }
}