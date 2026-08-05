using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Payments;
using EImece.Domain.Core.Cart;

namespace EImece.Web.Models;

public sealed class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
}

public sealed class MainPageBannerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Link { get; set; }
    public int? MainImageId { get; set; }
}

public sealed class HomePageViewModel
{
    public IReadOnlyList<MainPageBannerViewModel> Banners { get; set; } = Array.Empty<MainPageBannerViewModel>();
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
    public string? Error { get; set; }
}

public sealed class ProductDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "Product";
    public string? ProductCode { get; set; }
    public decimal? Price { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
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
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public sealed class ProductSearchViewModel
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public IReadOnlyList<ProductCategory> Categories { get; set; } = Array.Empty<ProductCategory>();
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public sealed class ProductTagViewModel
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public sealed class StoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = "hikaye";
}

public sealed class StoryDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? AuthorName { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = "hikaye";
    public int CategoryId { get; set; }
    public string? Notice { get; set; }
}

public sealed class StoryCategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Notice { get; set; }
    public IReadOnlyList<StoryListItemViewModel> Stories { get; set; } = Array.Empty<StoryListItemViewModel>();
}

public sealed class StoryTagViewModel
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string? Notice { get; set; }
    public IReadOnlyList<StoryListItemViewModel> Stories { get; set; } = Array.Empty<StoryListItemViewModel>();
}

public sealed class PageDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PageTheme { get; set; }
    public string? MetaKeywords { get; set; }
}

public sealed class InfoPageViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InfoKey { get; set; } = string.Empty;
}

public sealed class OrderListItemViewModel
{
    public int Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? PaymentStatus { get; set; }
    public string? Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public int ProductCount { get; set; }
}

public sealed class CustomerAccountViewModel
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IReadOnlyList<OrderListItemViewModel> Orders { get; set; } = Array.Empty<OrderListItemViewModel>();
}

public sealed class ManageIndexViewModel
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool HasPassword { get; set; }
    public string? StatusMessage { get; set; }
}

public sealed class ErrorPageViewModel
{
    public int StatusCode { get; set; }
    public string Title { get; set; } = "Error";
    public string? RequestedUrl { get; set; }
}

public sealed class CargoTrackingViewModel
{
    public string? OrderNumber { get; set; }
    public Order? Order { get; set; }
}

public sealed class ThankYouViewModel
{
    public Order? Order { get; set; }
    public string? Notice { get; set; }
}

public sealed class CheckoutBillingViewModel
{
    public CartState Cart { get; set; } = new();
    public Customer Customer { get; set; } = new();
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
