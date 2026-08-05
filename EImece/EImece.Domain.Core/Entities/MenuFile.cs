using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class MenuFile : BaseEntity
{
    [ForeignKey(nameof(Menu))]
    public int MenuId { get; set; }

    [ForeignKey(nameof(FileStorage))]
    public int FileStorageId { get; set; }

    public FileStorage? FileStorage { get; set; }
    public Menu? Menu { get; set; }
}
