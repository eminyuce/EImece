namespace EImece.Domain.Core.Entities;

public class StoryCategory : BaseContent
{
    public string? PageTheme { get; set; }
    public ICollection<Story> Stories { get; set; } = new HashSet<Story>();
}
