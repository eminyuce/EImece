using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class Tag : BaseEntity
{
    [ForeignKey(nameof(TagCategory))]
    public int TagCategoryId { get; set; }

    public TagCategory? TagCategory { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; } = new HashSet<ProductTag>();
    public ICollection<StoryTag> StoryTags { get; set; } = new HashSet<StoryTag>();
    public ICollection<FileStorageTag> FileStorageTags { get; set; } = new HashSet<FileStorageTag>();
}
