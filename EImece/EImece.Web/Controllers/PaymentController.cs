using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Cart/checkout shells only — Iyzico PlaceOrder/PaymentResult deferred to Phase 8.
/// </summary>
public sealed class PaymentController : BaseController
{
    public PaymentController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    [HttpGet]
    public IActionResult Index()
        => Placeholder("Shopping cart", "Cart shell — full checkout migrates in Phase 8.");

    [HttpGet]
    public IActionResult ShoppingCart()
        => Placeholder("Shopping cart", "ShoppingCart action shell.");

    [HttpGet]
    public IActionResult BuyNow(string categoryName, string? id)
        => Placeholder("Buy now", $"BuyNow route /b/{categoryName}/{id} — Iyzico not wired yet.", new { categoryName, id });

    [HttpGet]
    public IActionResult Checkout()
        => Placeholder("Checkout", "Checkout shell — payment integration in Phase 8.");
}
