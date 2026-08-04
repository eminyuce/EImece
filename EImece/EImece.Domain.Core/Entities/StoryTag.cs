using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class StoryTag : IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Story))]
    public int StoryId { get; set; }

    [ForeignKey(nameof(Tag))]
    public int TagId { get; set; }

    public Story? Story { get; set; }
    public Tag? Tag { get; set; }
}
