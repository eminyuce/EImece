using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class ProductCategory : BaseContent
{
    public int ParentId { get; set; }
    public bool MainPage { get; set; }
    public string? ShortDescription { get; set; }

    [ForeignKey(nameof(Template))]
    public int? TemplateId { get; set; }

    public Template? Template { get; set; }
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
}
