using EImece.Domain.Core.Payments;

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

public sealed class PaymentCheckoutViewModel
{
    public string Title { get; set; } = "Checkout";
    public string? Notice { get; set; }
    public bool IsIyzicoConfigured { get; set; }
    public string IyzicoBaseUrl { get; set; } = string.Empty;
    public string? OrderGuid { get; set; }
    public string? CategoryName { get; set; }
    public string? ProductId { get; set; }
    public CheckoutBuyer Buyer { get; set; } = new();
    public CheckoutAddress Address { get; set; } = new();
    public List<CheckoutBasketItem> BasketItems { get; set; } = [];
    public CheckoutInitializeResult? InitializeResult { get; set; }
}

public sealed class PaymentResultViewModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? OrderGuid { get; set; }
    public string? Token { get; set; }
    public CheckoutRetrieveResult? RetrieveResult { get; set; }
}
