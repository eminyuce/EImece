using EImece.Domain.Core.Email;
using EImece.Domain.Core.Payments;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Cart shell + Iyzico Checkout Form initialize/retrieve (Phase 8).
/// Full shopping-cart session / order persistence ports incrementally after this vertical.
/// </summary>
public sealed class PaymentController : BaseController
{
    private readonly IIyzicoPaymentService _iyzico;
    private readonly IEmailSender _email;
    private readonly IEmailTemplateRenderer _templates;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IOptions<EImeceOptions> siteOptions,
        IIyzicoPaymentService iyzico,
        IEmailSender email,
        IEmailTemplateRenderer templates,
        ILogger<PaymentController> logger)
        : base(siteOptions)
    {
        _iyzico = iyzico;
        _email = email;
        _templates = templates;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
        => View();

    [HttpGet]
    public IActionResult ShoppingCart()
        => View("Index");

    [HttpGet]
    public IActionResult Checkout()
    {
        var model = BuildDemoCheckoutViewModel();
        return View(model);
    }

    [HttpGet]
    public IActionResult BuyNow(string categoryName, string? id)
    {
        var model = BuildDemoCheckoutViewModel();
        model.Title = "Buy now";
        model.Notice = $"BuyNow route /b/{categoryName}/{id} — uses demo basket until cart session is ported.";
        model.CategoryName = categoryName;
        model.ProductId = id;
        return View("Checkout", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CancellationToken cancellationToken)
    {
        var model = BuildDemoCheckoutViewModel();
        if (!_iyzico.IsConfigured)
        {
            model.Notice = "Iyzico is not configured. Set Iyzico:ApiKey and Iyzico:SecretKey (sandbox) to initialize Checkout Form.";
            return View("Checkout", model);
        }

        var orderGuid = Guid.NewGuid().ToString("N");
        var conversationId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var callbackUrl = Url.Action(
            nameof(PaymentResult),
            "Payment",
            new { o = orderGuid },
            Request.Scheme,
            Request.Host.Value) ?? string.Empty;

        var request = new CheckoutInitializeRequest
        {
            OrderGuid = orderGuid,
            ConversationId = conversationId,
            CallbackUrl = callbackUrl,
            Buyer = model.Buyer,
            ShippingAddress = model.Address,
            BillingAddress = model.Address,
            BasketItems = model.BasketItems,
            Price = model.BasketItems.Sum(i => i.Price),
            PaidPrice = model.BasketItems.Sum(i => i.Price)
        };

        var result = await _iyzico.InitializeCheckoutFormAsync(request, cancellationToken).ConfigureAwait(false);
        model.InitializeResult = result;
        model.OrderGuid = orderGuid;

        if (!result.Success)
        {
            model.Notice = result.ErrorMessage ?? "Iyzico initialize failed.";
            _logger.LogWarning("Iyzico PlaceOrder failed: {Status} {Error}", result.Status, result.ErrorMessage);
        }

        return View("Checkout", model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken] // Iyzico server callback posts token without antiforgery.
    public async Task<IActionResult> PaymentResult(string? token, string? o, CancellationToken cancellationToken)
    {
        var vm = new PaymentResultViewModel
        {
            OrderGuid = o,
            Token = token
        };

        if (string.IsNullOrWhiteSpace(token))
        {
            vm.Success = false;
            vm.Message = "Missing Iyzico token.";
            return View(vm);
        }

        if (!_iyzico.IsConfigured)
        {
            vm.Success = false;
            vm.Message = "Iyzico is not configured on this host.";
            return View(vm);
        }

        var result = await _iyzico.RetrieveCheckoutFormAsync(token, cancellationToken).ConfigureAwait(false);
        vm.RetrieveResult = result;
        vm.Success = result.Success
            && string.Equals(result.PaymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase);
        vm.Message = vm.Success
            ? "Payment successful."
            : (result.ErrorMessage ?? result.PaymentStatus ?? result.Status ?? "Payment not successful.");

        if (vm.Success)
        {
            var body = await _templates.RenderAsync(
                "<p>Hello {{ Name }},</p><p>Order <strong>{{ OrderGuid }}</strong> payment {{ PaymentId }} is confirmed.</p>",
                new { Name = "Customer", OrderGuid = o ?? result.BasketId, PaymentId = result.PaymentId },
                cancellationToken).ConfigureAwait(false);

            await _email.SendAsync(new EmailMessage
            {
                ToAddress = "demo@eimece.local",
                Subject = $"Order confirmation {result.ConversationId}",
                HtmlBody = body
            }, cancellationToken).ConfigureAwait(false);
        }

        return View(vm);
    }

    private PaymentCheckoutViewModel BuildDemoCheckoutViewModel()
    {
        var items = new List<CheckoutBasketItem>
        {
            new()
            {
                Id = "DEMO-1",
                Name = "EImece demo product",
                Category1 = "Demo",
                Category2 = "Physical",
                Price = 10.00m
            }
        };

        return new PaymentCheckoutViewModel
        {
            Title = "Checkout",
            IsIyzicoConfigured = _iyzico.IsConfigured,
            IyzicoBaseUrl = _iyzico.BaseUrl,
            BasketItems = items,
            Buyer = new CheckoutBuyer(),
            Address = new CheckoutAddress(),
            Notice = _iyzico.IsConfigured
                ? "Demo basket ready — Place order calls Iyzico sandbox Checkout Form."
                : "Configure Iyzico sandbox keys to enable Place order."
        };
    }
}
