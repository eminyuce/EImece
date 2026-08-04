using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public class Product : BaseContent
{
    public override string Name { get; set; } = string.Empty;
    public string? NameShort { get; set; }
    public string? NameLong { get; set; }

    [ForeignKey(nameof(ProductCategory))]
    public int ProductCategoryId { get; set; }

    public int? BrandId { get; set; }
    public bool MainPage { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public bool IsCampaign { get; set; }
    public string? ProductColorOptions { get; set; }
    public string State { get; set; } = string.Empty;
    public string? ProductSizeOptions { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public double Rating { get; set; }

    public ProductCategory? ProductCategory { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductComment> ProductComments { get; set; } = new HashSet<ProductComment>();
    public ICollection<ProductFile> ProductFiles { get; set; } = new HashSet<ProductFile>();
    public ICollection<ProductTag> ProductTags { get; set; } = new HashSet<ProductTag>();
    public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new HashSet<ProductSpecification>();
}
