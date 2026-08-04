using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public abstract class BaseEntity : IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    [Column("Name")]
    public virtual string Name { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public int Position { get; set; }
    public int Lang { get; set; }
}
