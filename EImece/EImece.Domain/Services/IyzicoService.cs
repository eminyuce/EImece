using EImece.Domain.Configuration;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Telemetry;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using IyzipayOptions = Iyzipay.Options;

namespace EImece.Domain.Services
{
    public class IyzicoService
    {
        private readonly ILogger<IyzicoService> _logger;
        private readonly IOptions<IyzicoOptions> _iyzicoOptions;

        public IyzicoService(ILogger<IyzicoService> logger, IOptions<IyzicoOptions> iyzicoOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iyzicoOptions = iyzicoOptions ?? throw new ArgumentNullException(nameof(iyzicoOptions));
        }

        public async Task<CheckoutForm> GetCheckoutFormAsync(RetrieveCheckoutFormRequest model)
        {
            using (var activity = StartPaymentActivity("callback"))
            {
                IyzipayOptions options = GetOptions();
                var request = new RetrieveCheckoutFormRequest();
                request.Token = model.Token;

                try
                {
                    // Await the SDK call instead of blocking on .Result. ConfigureAwait(false) keeps this
                    // domain-layer code off the ASP.NET request context.
                    var result = await CheckoutForm.Retrieve(request, options).ConfigureAwait(false);
                    if (result == null)
                    {
                        _logger.LogError("Iyzico CheckoutForm retrieve returned null.");
                    }
                    else if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError(
                            "Iyzico CheckoutForm retrieve rejected status={0} errorCode={1} errorMessage={2} errorGroup={3} conversationId={4} basketId={5} paymentStatus={6}",
                            result.Status,
                            result.ErrorCode,
                            result.ErrorMessage,
                            result.ErrorGroup,
                            result.ConversationId,
                            result.BasketId,
                            result.PaymentStatus);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Iyzico CheckoutForm retrieve status={0} conversationId={1} basketId={2} paymentStatus={3} paidPrice={4} currency={5}",
                            result.Status,
                            result.ConversationId,
                            result.BasketId,
                            result.PaymentStatus,
                            result.PaidPrice,
                            result.Currency);
                    }
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    _logger.LogError(ex, "Iyzico CheckoutForm retrieve threw before a response was returned.");
                    throw;
                }
            }
        }

        public async Task<CheckoutFormInitialize> CreateCheckoutFormInitializeAsync(ShoppingCartSession shoppingCart, string userId, string actionName = "PaymentResult", string callbackUrl = null)
        {
            _logger.LogInformation("Initializing CheckoutForm for user: " + userId);

            // Validation checks
            if (shoppingCart == null)
            {
                _logger.LogError("ShoppingCartSession cannot be null");
                throw new ArgumentNullException("ShoppingCartSession cannot be null");
            }
            if (shoppingCart.ShoppingCartItems.IsEmpty())
            {
                _logger.LogError("ShoppingCartSession.ShoppingCartItems cannot be null");
                throw new ArgumentNullException("ShoppingCartSession.ShoppingCartItems cannot be null");
            }
            if (shoppingCart.Customer == null)
            {
                _logger.LogError("ShoppingCartSession.Customer cannot be null");
                throw new ArgumentNullException("ShoppingCartSession.Customer cannot be null");
            }

            // Configure iyzico options
            IyzipayOptions options = GetOptions();

            // Build callback URL
            string orderNumber = GeneralHelper.GenerateOrderNumber();
            if (string.IsNullOrEmpty(callbackUrl))
            {
                _logger.LogDebug("Building callback URL for Payment Result...");
                string o = WebUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(shoppingCart.OrderGuid));
                string u = WebUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(userId));
                var baseUrl = EntityExtension.GetAbsoluteApplicationBaseUrl(AppConfig.HttpProtocol);
                callbackUrl = $"{baseUrl}/payment/{actionName}?o={o}&u={u}&orderNumber={orderNumber}";
            }

            // Initialize request
            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = orderNumber,
                Currency = Currency.TRY.ToString(),
                BasketId = shoppingCart.OrderGuid,
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = callbackUrl,
                EnabledInstallments = GetEnabledInstallments()
            };

            // Populate buyer details
            var customer = shoppingCart.Customer;
            request.Buyer = new Buyer
            {
                Id = customer.Id.ToStr(),
                Name = customer.Name,
                Surname = customer.Surname,
                GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber),
                Email = customer.Email == null ? null : customer.Email.Trim(),
                IdentityNumber = GeneralHelper.CheckIdentityNumber(customer.IdentityNumber),
                LastLoginDate = customer.UpdatedDate.ToString(Constants.IyzicoDateTimeFormat),
                RegistrationDate = customer.CreatedDate.ToString(Constants.IyzicoDateTimeFormat),
                RegistrationAddress = customer.RegistrationAddress,
                Ip = customer.Ip,
                City = customer.City,
                Country = customer.Country,
                ZipCode = customer.ZipCode
            };

            // Populate shipping and billing addresses
            if (shoppingCart.Customer.IsSameAsShippingAddress)
            {
                Address sharedAddress = new Address
                {
                    ContactName = shoppingCart.Customer.FullName,
                    City = shoppingCart.ShippingAddress.City,
                    Country = shoppingCart.ShippingAddress.Country,
                    Description = shoppingCart.ShippingAddress.Description,
                    ZipCode = shoppingCart.ShippingAddress.ZipCode
                };
                request.ShippingAddress = sharedAddress;
                request.BillingAddress = sharedAddress;
            }
            else
            {
                request.ShippingAddress = new Address
                {
                    ContactName = shoppingCart.Customer.FullName,
                    City = shoppingCart.ShippingAddress.City,
                    Country = shoppingCart.ShippingAddress.Country,
                    Description = shoppingCart.ShippingAddress.Description,
                    ZipCode = shoppingCart.ShippingAddress.ZipCode
                };

                request.BillingAddress = new Address
                {
                    ContactName = shoppingCart.Customer.FullName,
                    City = shoppingCart.BillingAddress.City,
                    Country = shoppingCart.BillingAddress.Country,
                    Description = shoppingCart.BillingAddress.Description,
                    ZipCode = shoppingCart.BillingAddress.ZipCode
                };
            }

            IyzicoCheckoutBasketMapper.ApplyCart(request, shoppingCart);
            _logger.LogDebug("Total Price: " + request.Price);
            _logger.LogDebug("TotalPriceWithCargoPrice: " + shoppingCart.TotalPriceWithCargoPrice);

            LogCheckoutFormRequest(options, request, customer.Email);

            // Execute the request
            _logger.LogDebug("Initializing CheckoutFormInitialize.Create for user: " + userId);
            using (var activity = StartPaymentActivity("authorize"))
            {
                activity?.SetTag("order.conversation_id", request.ConversationId);
                try
                {
                    // HttpContext.Current was read synchronously above (before this await), so ConfigureAwait(false)
                    // here is safe and avoids parking the request thread on the payment gateway round-trip.
                    var result = await CheckoutFormInitialize.Create(request, options).ConfigureAwait(false);
                    LogCheckoutFormResult(result);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    _logger.LogError(ex, "Iyzico CheckoutForm initialize threw before a response was returned.");
                    throw;
                }
            }
        }

        public async Task<CheckoutFormInitialize> CreateCheckoutFormInitializeBuyNowAsync(BuyNowModel buyNowModel, string callbackUrl = null)
        {
            _logger.LogInformation("Initializing CheckoutForm for BuyNow with OrderGuid: " + buyNowModel.OrderGuid);

            IyzipayOptions options = GetOptions();
            var customer = buyNowModel.Customer;

            if (string.IsNullOrEmpty(callbackUrl))
            {
                _logger.LogDebug("Building callback URL for BuyNow Payment Result...");
                string o = WebUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(buyNowModel.OrderGuid));
                var baseUrl = EntityExtension.GetAbsoluteApplicationBaseUrl(AppConfig.HttpProtocol);
                callbackUrl = $"{baseUrl}/payment/buynowpaymentresult?o={o}";
            }

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = buyNowModel.ConversationId,
                Currency = Currency.TRY.ToString(),
                BasketId = buyNowModel.OrderGuid,
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = callbackUrl,
                EnabledInstallments = GetEnabledInstallments()
            };

            _logger.LogDebug("CheckoutFormInitializeRequest object populated");

            // Buyer details
            Buyer buyer = new Buyer
            {
                Id = customer.Id.ToStr(),
                Name = customer.Name,
                Surname = customer.Surname,
                GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber),
                Email = customer.Email == null ? null : customer.Email.Trim(),
                IdentityNumber = GeneralHelper.CheckIdentityNumber(customer.IdentityNumber),
                LastLoginDate = customer.UpdatedDate.ToString(Constants.IyzicoDateTimeFormat),
                RegistrationDate = customer.CreatedDate.ToString(Constants.IyzicoDateTimeFormat),
                RegistrationAddress = customer.RegistrationAddress,
                Ip = customer.Ip,
                City = customer.City,
                Country = customer.Country,
                ZipCode = customer.ZipCode
            };
            request.Buyer = buyer;

            // Shipping & Billing address
            Address shippingAddress = new Address
            {
                ContactName = customer.FullName,
                City = buyNowModel.ShippingAddress.City,
                Country = buyNowModel.ShippingAddress.Country,
                Description = buyNowModel.ShippingAddress.Description,
                ZipCode = buyNowModel.ShippingAddress.ZipCode
            };
            request.ShippingAddress = shippingAddress;
            request.BillingAddress = shippingAddress;

            // Basket item
            List<BasketItem> basketItems = new List<BasketItem>();
            decimal totalPrice = 0;

            var item = buyNowModel.ProductDetailViewModel.ProductDto;
            BasketItem firstBasketItem = new BasketItem
            {
                Id = item.ProductCode,
                Name = item.NameLong,
                Category1 = item.ProductCategoryName,
                Category2 = AppConfig.ShoppingCartItemCategory2,
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Price = decimal.Round(item.Price, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".")
            };
            totalPrice += item.Price;
            basketItems.Add(firstBasketItem);

            _logger.LogDebug("Total Price for BuyNow: " + totalPrice);
            request.Price = decimal.Round(totalPrice, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            request.PaidPrice = decimal.Round(item.Price, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");

            request.BasketItems = basketItems;

            LogCheckoutFormRequest(options, request, customer.Email);

            using (var activity = StartPaymentActivity("authorize_buynow"))
            {
                activity?.SetTag("order.conversation_id", request.ConversationId);
                try
                {
                    var result = await CheckoutFormInitialize.Create(request, options).ConfigureAwait(false);
                    LogCheckoutFormResult(result);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    _logger.LogError(ex, "Iyzico CheckoutForm BuyNow initialize threw before a response was returned.");
                    throw;
                }
            }
        }

        private void LogCheckoutFormRequest(IyzipayOptions options, CreateCheckoutFormInitializeRequest request, string storeEmail)
        {
            var buyer = request != null ? request.Buyer : null;
            var identity = buyer != null ? buyer.IdentityNumber : null;
            var basketSummary = FormatBasketSummary(request);
            _logger.LogInformation(
                "Iyzico CheckoutForm request host={0} locale={1} currency={2} conversationId={3} basketId={4} price={5} paidPrice={6} items={7} storeEmail={8} buyerEmail={9} gsm={10} identity={11}",
                options != null ? options.BaseUrl : null,
                request != null ? request.Locale : null,
                request != null ? request.Currency : null,
                request != null ? request.ConversationId : null,
                request != null ? request.BasketId : null,
                request != null ? request.Price : null,
                request != null ? request.PaidPrice : null,
                basketSummary,
                storeEmail,
                buyer != null ? buyer.Email : null,
                buyer != null ? buyer.GsmNumber : null,
                MaskIdentityForLog(identity));
        }

        private void LogCheckoutFormResult(CheckoutFormInitialize result)
        {
            if (result == null)
            {
                _logger.LogError("Iyzico CheckoutForm response was null.");
                return;
            }

            var hasContent = !string.IsNullOrEmpty(result.CheckoutFormContent);
            var failed = !string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase);
            var message =
                "Iyzico CheckoutForm response status={0} errorCode={1} errorMessage={2} errorGroup={3} locale={4} conversationId={5} hasContent={6} hasToken={7}";
            if (failed)
            {
                _logger.LogError(
                    message,
                    result.Status,
                    result.ErrorCode,
                    result.ErrorMessage,
                    result.ErrorGroup,
                    result.Locale,
                    result.ConversationId,
                    hasContent,
                    !string.IsNullOrEmpty(result.Token));
            }
            else
            {
                _logger.LogInformation(
                    message,
                    result.Status,
                    result.ErrorCode,
                    result.ErrorMessage,
                    result.ErrorGroup,
                    result.Locale,
                    result.ConversationId,
                    hasContent,
                    !string.IsNullOrEmpty(result.Token));
            }
        }

        private static string FormatBasketSummary(CreateCheckoutFormInitializeRequest request)
        {
            if (request == null || request.BasketItems == null || request.BasketItems.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>(request.BasketItems.Count);
            foreach (var item in request.BasketItems)
            {
                parts.Add((item.Id ?? "?") + "=" + (item.Price ?? "0"));
            }

            return request.BasketItems.Count + ":" + string.Join(",", parts);
        }

        private static string MaskIdentityForLog(string identityNumber)
        {
            if (string.IsNullOrEmpty(identityNumber))
            {
                return "empty";
            }

            var last = identityNumber.Length <= 3
                ? identityNumber
                : identityNumber.Substring(identityNumber.Length - 3);
            return "len=" + identityNumber.Length + ",last3=" + last;
        }

        private IyzipayOptions GetOptions()
        {
            _logger.LogDebug("Fetching Iyzico API options...");
            var config = _iyzicoOptions.Value;
            var apiKey = config.ApiKey;
            var secretKey = config.SecretKey;

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogError("Iyzico API credentials are not configured. Set IyzicoApiKey and IyzicoSecretKey via environment variables or AppSettings.");
                throw new InvalidOperationException(
                    "Iyzico payment gateway is not configured. Both IyzicoApiKey and IyzicoSecretKey must be set in secure configuration.");
            }

            IyzipayOptions options = new IyzipayOptions
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
                BaseUrl = config.BaseUrl
            };
            _logger.LogDebug("Iyzico API options fetched successfully.");
            return options;
        }

        private static List<int> GetEnabledInstallments()
        {
            var settingService = DomainServiceProvider.GetService<EImece.Domain.Services.IServices.ISettingService>();
            var raw = settingService?.GetSettingByKey(Constants.IyzicoEnabledInstallments);
            if (string.IsNullOrWhiteSpace(raw))
            {
                raw = Constants.DefaultIyzicoEnabledInstallments;
            }

            var enabledInstallments = new List<int>();
            foreach (var item in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                enabledInstallments.Add(item.Trim().ToInt());
            }
            return enabledInstallments.Count > 0 ? enabledInstallments : new List<int> { 1, 2, 4, 6, 9 };
        }

        private static Activity StartPaymentActivity(string operation)
        {
            var activity = OpenTelemetryBootstrap.ActivitySource?.StartActivity(
                "iyzico." + operation,
                ActivityKind.Client);

            if (activity == null)
            {
                return null;
            }

            activity.SetTag(ActivityTags.PaymentProvider, "iyzico");
            activity.SetTag(ActivityTags.PaymentOperation, operation);
            activity.SetTag(ActivityTags.CorrelationId, CorrelationIdContext.Current);
            activity.SetTag(ActivityTags.ServerAddress, "iyzico");
            return activity;
        }
    }
}