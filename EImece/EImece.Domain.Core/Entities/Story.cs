using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class Story : BaseContent
{
    [ForeignKey(nameof(StoryCategory))]
    public int StoryCategoryId { get; set; }
    public bool MainPage { get; set; }
    public string? AuthorName { get; set; }
    public bool IsFeaturedStory { get; set; }
    public string? ShortDescription { get; set; }

    public StoryCategory? StoryCategory { get; set; }
    public ICollection<StoryTag> StoryTags { get; set; } = new HashSet<StoryTag>();
    public ICollection<StoryFile> StoryFiles { get; set; } = new HashSet<StoryFile>();
}
