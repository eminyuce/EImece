using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class StoryFile : BaseEntity
{
    [ForeignKey(nameof(Story))]
    public int StoryId { get; set; }

    [ForeignKey(nameof(FileStorage))]
    public int FileStorageId { get; set; }

    public FileStorage? FileStorage { get; set; }
    public Story? Story { get; set; }
}
