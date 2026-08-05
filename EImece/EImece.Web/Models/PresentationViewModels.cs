namespace EImece.Web.Models;

public sealed class ProductDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "Product";
    public string? ProductCode { get; set; }
    public decimal? Price { get; set; }
    public string CategoryName { get; set; } = "Category";
    public string CategorySlug { get; set; } = "category";
    public int CategoryId { get; set; } = 1;
    public string? Summary { get; set; }
    public string? Notice { get; set; }
}

public sealed class CategoryShellViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "Category";
    public string? Summary { get; set; }
    public string? Notice { get; set; }
}
