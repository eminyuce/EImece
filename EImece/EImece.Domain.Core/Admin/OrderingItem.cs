using System.Text.Json.Serialization;

namespace EImece.Domain.Core.Admin;

public sealed class OrderingItem
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Position { get; set; }

    public bool IsActive { get; set; }
}

public sealed class CategoryTreeNode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public int ProductCount { get; set; }
    public int Level { get; set; }
    public List<CategoryTreeNode> Children { get; set; } = [];
}
