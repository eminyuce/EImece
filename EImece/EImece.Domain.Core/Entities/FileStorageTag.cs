using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class FileStorageTag : IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(FileStorage))]
    public int FileStorageId { get; set; }

    [ForeignKey(nameof(Tag))]
    public int TagId { get; set; }

    public Tag? Tag { get; set; }
    public FileStorage? FileStorage { get; set; }
}
