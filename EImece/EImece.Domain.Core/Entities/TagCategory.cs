namespace EImece.Domain.Core.Entities;

public class TagCategory : BaseEntity
{
    public ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
}
