namespace EImece.Domain.Core.Payments;

public sealed class CheckoutBuyer
{
    public string Id { get; set; } = "1";
    public string Name { get; set; } = "Demo";
    public string Surname { get; set; } = "Buyer";
    public string Email { get; set; } = "demo@eimece.local";
    public string GsmNumber { get; set; } = "+905350000000";
    public string IdentityNumber { get; set; } = "11111111111";
    public string RegistrationAddress { get; set; } = "Demo address";
    public string Ip { get; set; } = "127.0.0.1";
    public string City { get; set; } = "Istanbul";
    public string Country { get; set; } = "Turkey";
    public string ZipCode { get; set; } = "34000";
}

public sealed class CheckoutAddress
{
    public string ContactName { get; set; } = "Demo Buyer";
    public string City { get; set; } = "Istanbul";
    public string Country { get; set; } = "Turkey";
    public string Description { get; set; } = "Demo shipping address";
    public string ZipCode { get; set; } = "34000";
}

public sealed class CheckoutBasketItem
{
    public string Id { get; set; } = "SKU-1";
    public string Name { get; set; } = "Demo product";
    public string Category1 { get; set; } = "General";
    public string Category2 { get; set; } = "Physical";
    public decimal Price { get; set; } = 1.00m;
}

public sealed class CheckoutInitializeRequest
{
    public string OrderGuid { get; set; } = Guid.NewGuid().ToString("N");
    public string ConversationId { get; set; } = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    public string CallbackUrl { get; set; } = string.Empty;
    public CheckoutBuyer Buyer { get; set; } = new();
    public CheckoutAddress ShippingAddress { get; set; } = new();
    public CheckoutAddress BillingAddress { get; set; } = new();
    public List<CheckoutBasketItem> BasketItems { get; set; } = [];
    public decimal Price { get; set; }
    public decimal PaidPrice { get; set; }
}

public sealed class CheckoutInitializeResult
{
    public bool Success { get; init; }
    public string? Status { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Token { get; init; }
    public string? CheckoutFormContent { get; init; }
    public string? PaymentPageUrl { get; init; }
    public string? ConversationId { get; init; }
}

public sealed class CheckoutRetrieveResult
{
    public bool Success { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentId { get; init; }
    public string? ErrorMessage { get; init; }
    public string? BasketId { get; init; }
    public string? ConversationId { get; init; }
    public string? CardFamily { get; init; }
    public int? Installment { get; init; }
}
