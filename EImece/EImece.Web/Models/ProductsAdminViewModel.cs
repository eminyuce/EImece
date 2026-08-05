using EImece.Domain.Core.Admin;
using EImece.Domain.Core.Enums;

namespace EImece.Web.Models;

public sealed class ProductsAdminViewModel
{
    public string? Search { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public string? SelectedCategoryName { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public string? Sort { get; set; }
    public string SortDir { get; set; } = "desc";

    public List<CategoryTreeNode> CategoryTree { get; set; } = [];
    public List<BrandFilterItem> Brands { get; set; } = [];
    public List<ProductAdminRow> Products { get; set; } = [];
    public IReadOnlyList<ProductStateOption> ProductStates { get; set; } = ProductStateOption.All;
}

public sealed class BrandFilterItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductStateOption
{
    public int Value { get; init; }
    public string Text { get; init; } = string.Empty;

    public static IReadOnlyList<ProductStateOption> All { get; } =
        Enum.GetValues<ProductState>()
            .Select(s => new ProductStateOption { Value = (int)s, Text = ProductStateLabels.ToTurkish(s) })
            .ToList();
}

public sealed class MoveProductsViewModel
{
    public int CategoryId { get; set; }
    public string? ProductIdList { get; set; }
    public int OldCategoryId { get; set; }
    public string? Message { get; set; }
    public List<CategoryTreeNode> CategoryTree { get; set; } = [];
    public List<ProductAdminRow> Products { get; set; } = [];
}
