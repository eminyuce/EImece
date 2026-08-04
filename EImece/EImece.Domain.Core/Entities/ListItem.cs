using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class ListItem : BaseEntity
{
    [ForeignKey(nameof(List))]
    public int ListId { get; set; }
    public string? Value { get; set; }
    public List? List { get; set; }
}
