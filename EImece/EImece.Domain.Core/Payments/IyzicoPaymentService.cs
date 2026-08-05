using EImece.Domain.Core.Configuration;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Core.Payments;

public sealed class IyzicoPaymentService : IIyzicoPaymentService
{
    private readonly IyzicoOptions _options;
    private readonly ILogger<IyzicoPaymentService> _logger;

    public IyzicoPaymentService(IOptions<IyzicoOptions> options, ILogger<IyzicoPaymentService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;
    public string BaseUrl => _options.BaseUrl;

    public async Task<CheckoutInitializeResult> InitializeCheckoutFormAsync(
        CheckoutInitializeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConfigured)
        {
            return new CheckoutInitializeResult
            {
                Success = false,
                Status = "not_configured",
                ErrorMessage = "Iyzico ApiKey/SecretKey are not configured."
            };
        }

        if (string.IsNullOrWhiteSpace(request.CallbackUrl))
        {
            return new CheckoutInitializeResult
            {
                Success = false,
                Status = "invalid_request",
                ErrorMessage = "CallbackUrl is required."
            };
        }

        if (request.BasketItems.Count == 0)
        {
            return new CheckoutInitializeResult
            {
                Success = false,
                Status = "invalid_request",
                ErrorMessage = "BasketItems cannot be empty."
            };
        }

        try
        {
            var options = CreateOptions();
            var apiRequest = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = request.ConversationId,
                Currency = Currency.TRY.ToString(),
                BasketId = request.OrderGuid,
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = request.CallbackUrl,
                EnabledInstallments = ParseInstallments(_options.EnabledInstallments),
                Price = FormatPrice(request.Price),
                PaidPrice = FormatPrice(request.PaidPrice),
                Buyer = new Buyer
                {
                    Id = request.Buyer.Id,
                    Name = request.Buyer.Name,
                    Surname = request.Buyer.Surname,
                    GsmNumber = request.Buyer.GsmNumber,
                    Email = request.Buyer.Email,
                    IdentityNumber = request.Buyer.IdentityNumber,
                    LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationAddress = request.Buyer.RegistrationAddress,
                    Ip = request.Buyer.Ip,
                    City = request.Buyer.City,
                    Country = request.Buyer.Country,
                    ZipCode = request.Buyer.ZipCode
                },
                ShippingAddress = ToAddress(request.ShippingAddress),
                BillingAddress = ToAddress(request.BillingAddress),
                BasketItems = request.BasketItems.Select(i => new BasketItem
                {
                    Id = i.Id,
                    Name = i.Name,
                    Category1 = i.Category1,
                    Category2 = i.Category2,
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = FormatPrice(i.Price)
                }).ToList()
            };

            var result = await CheckoutFormInitialize.Create(apiRequest, options).ConfigureAwait(false);
            var ok = string.Equals(result.Status, Status.SUCCESS.ToString(), StringComparison.OrdinalIgnoreCase);

            if (!ok)
            {
                _logger.LogWarning(
                    "Iyzico CheckoutFormInitialize failed: {Status} {Error}",
                    result.Status,
                    result.ErrorMessage);
            }

            return new CheckoutInitializeResult
            {
                Success = ok,
                Status = result.Status,
                ErrorMessage = result.ErrorMessage,
                Token = result.Token,
                CheckoutFormContent = result.CheckoutFormContent,
                PaymentPageUrl = result.PaymentPageUrl,
                ConversationId = result.ConversationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iyzico CheckoutFormInitialize threw");
            return new CheckoutInitializeResult
            {
                Success = false,
                Status = "exception",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<CheckoutRetrieveResult> RetrieveCheckoutFormAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsConfigured)
        {
            return new CheckoutRetrieveResult
            {
                Success = false,
                Status = "not_configured",
                ErrorMessage = "Iyzico ApiKey/SecretKey are not configured."
            };
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return new CheckoutRetrieveResult
            {
                Success = false,
                Status = "invalid_request",
                ErrorMessage = "Token is required."
            };
        }

        try
        {
            var options = CreateOptions();
            var request = new RetrieveCheckoutFormRequest { Token = token };
            var form = await CheckoutForm.Retrieve(request, options).ConfigureAwait(false);
            var ok = string.Equals(form.Status, Status.SUCCESS.ToString(), StringComparison.OrdinalIgnoreCase);

            return new CheckoutRetrieveResult
            {
                Success = ok,
                Status = form.Status,
                PaymentStatus = form.PaymentStatus,
                PaymentId = form.PaymentId,
                ErrorMessage = form.ErrorMessage,
                BasketId = form.BasketId,
                ConversationId = form.ConversationId,
                CardFamily = form.CardFamily,
                Installment = form.Installment
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iyzico CheckoutForm.Retrieve threw");
            return new CheckoutRetrieveResult
            {
                Success = false,
                Status = "exception",
                ErrorMessage = ex.Message
            };
        }
    }

    private Iyzipay.Options CreateOptions() => new()
    {
        ApiKey = _options.ApiKey,
        SecretKey = _options.SecretKey,
        BaseUrl = _options.BaseUrl
    };

    private static Address ToAddress(CheckoutAddress address) => new()
    {
        ContactName = address.ContactName,
        City = address.City,
        Country = address.Country,
        Description = address.Description,
        ZipCode = address.ZipCode
    };

    private static string FormatPrice(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static List<int> ParseInstallments(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [1, 2, 3, 6, 9];
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .ToList();
    }
}
