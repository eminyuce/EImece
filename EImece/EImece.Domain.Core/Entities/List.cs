namespace EImece.Domain.Core.Entities;

public class List : BaseEntity
{
    public bool IsService { get; set; }
    public bool IsValues { get; set; }
    public ICollection<ListItem> ListItems { get; set; } = new HashSet<ListItem>();
}
