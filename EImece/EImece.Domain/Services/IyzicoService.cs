using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Observability.Telemetry;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Services
{
    public class IyzicoService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<CheckoutForm> GetCheckoutFormAsync(RetrieveCheckoutFormRequest model)
        {
            using (var activity = StartPaymentActivity("callback"))
            {
                Options options = GetOptions();
                var request = new RetrieveCheckoutFormRequest();
                request.Token = model.Token;

                try
                {
                    // Await the SDK call instead of blocking on .Result. ConfigureAwait(false) keeps this
                    // domain-layer code off the ASP.NET request context.
                    var result = await CheckoutForm.Retrieve(request, options).ConfigureAwait(false);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    throw;
                }
            }
        }

        public async Task<CheckoutFormInitialize> CreateCheckoutFormInitializeAsync(ShoppingCartSession shoppingCart, string userId, string actionName = "PaymentResult", string callbackUrl = null)
        {
            Logger.Info("Initializing CheckoutForm for user: " + userId);

            // Validation checks
            if (shoppingCart == null)
            {
                Logger.Error("ShoppingCartSession cannot be null");
                throw new ArgumentNullException("ShoppingCartSession cannot be null");
            }
            if (shoppingCart.ShoppingCartItems.IsEmpty())
            {
                Logger.Error("ShoppingCartSession.ShoppingCartItems cannot be null");
                throw new ArgumentNullException("ShoppingCartSession.ShoppingCartItems cannot be null");
            }
            if (shoppingCart.Customer == null)
            {
                Logger.Error("ShoppingCartSession.Customer cannot be null");
                throw new ArgumentNullException("ShoppingCartSession.Customer cannot be null");
            }

            // Configure iyzico options
            Options options = GetOptions();

            // Build callback URL
            string orderNumber = GeneralHelper.GenerateOrderNumber();
            if (string.IsNullOrEmpty(callbackUrl))
            {
                Logger.Debug("Building callback URL for Payment Result...");
                string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(shoppingCart.OrderGuid));
                string u = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(userId));
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var requestContext = HttpContext.Current.Request.RequestContext;
                    callbackUrl = new UrlHelper(requestContext).Action(actionName,
                                                       "Payment",
                                                       new { o, u, orderNumber },
                                                       AppConfig.HttpProtocol);
                }
                else
                {
                    callbackUrl = $"/Payment/{actionName}?o={o}&u={u}&orderNumber={orderNumber}";
                }
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
                EnabledInstallments = AppConfig.IyzicoEnabledInstallments
            };

            // Populate buyer details
            var customer = shoppingCart.Customer;
            request.Buyer = new Buyer
            {
                Id = customer.Id.ToStr(),
                Name = customer.Name,
                Surname = customer.Surname,
                GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber),
                Email = customer.Email,
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

            // Populate basket items and calculate total price
            List<BasketItem> basketItems = new List<BasketItem>();
            decimal totalPrice = 0;

            foreach (ShoppingCartItem shoppingCartItem in shoppingCart.ShoppingCartItems)
            {
                var item = shoppingCartItem.Product;
                BasketItem basketItem = new BasketItem
                {
                    Id = item.ProductCode,
                    Name = item.Name,
                    Category1 = item.CategoryName,
                    Category2 = AppConfig.ShoppingCartItemCategory2,
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = CurrencyHelper.CurrencySignForIyizo(item.Price)
                };
                totalPrice += item.Price;
                basketItems.Add(basketItem);
            }

            Logger.Debug("Total Price: " + totalPrice);
            Logger.Debug("TotalPriceWithCargoPrice: " + shoppingCart.TotalPriceWithCargoPrice);

            // Set price fields
            request.Price = CurrencyHelper.CurrencySignForIyizo(totalPrice);
            request.PaidPrice = CurrencyHelper.CurrencySignForIyizo(shoppingCart.TotalPriceWithCargoPrice);
            request.BasketItems = basketItems;

            // Log prices and request details (never log full payment payloads / secrets).
            Logger.Debug("Total Price after CurrencySignForIyizo: " + request.Price);
            Logger.Debug("Shipping & Paid Price after CurrencySignForIyizo: " + request.PaidPrice);
            Logger.Info(SensitiveDataMasker.Mask(
                "Iyzico Request prepared for CheckoutFormInitialization: ConversationId="
                + request.ConversationId
                + " BasketId="
                + request.BasketId));

            // Execute the request
            Logger.Debug("Initializing CheckoutFormInitialize.Create for user: " + userId);
            using (var activity = StartPaymentActivity("authorize"))
            {
                activity?.SetTag("order.conversation_id", request.ConversationId);
                try
                {
                    // HttpContext.Current was read synchronously above (before this await), so ConfigureAwait(false)
                    // here is safe and avoids parking the request thread on the payment gateway round-trip.
                    var result = await CheckoutFormInitialize.Create(request, options).ConfigureAwait(false);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    throw;
                }
            }
        }

        public async Task<CheckoutFormInitialize> CreateCheckoutFormInitializeBuyNowAsync(BuyNowModel buyNowModel, string callbackUrl = null)
        {
            Logger.Info("Initializing CheckoutForm for BuyNow with OrderGuid: " + buyNowModel.OrderGuid);

            Options options = GetOptions();
            var customer = buyNowModel.Customer;

            if (string.IsNullOrEmpty(callbackUrl))
            {
                Logger.Debug("Building callback URL for BuyNow Payment Result...");
                string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(buyNowModel.OrderGuid));
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var requestContext = HttpContext.Current.Request.RequestContext;
                    callbackUrl = new UrlHelper(requestContext).Action("BuyNowPaymentResult",
                                                       "Payment",
                                                       new { o },
                                                       AppConfig.HttpProtocol);
                }
                else
                {
                    callbackUrl = $"/Payment/BuyNowPaymentResult?o={o}";
                }
            }

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = buyNowModel.ConversationId,
                Currency = Currency.TRY.ToString(),
                BasketId = buyNowModel.OrderGuid,
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = callbackUrl,
                EnabledInstallments = AppConfig.IyzicoEnabledInstallments
            };

            Logger.Debug("CheckoutFormInitializeRequest object populated");

            // Buyer details
            Buyer buyer = new Buyer
            {
                Id = customer.Id.ToStr(),
                Name = customer.Name,
                Surname = customer.Surname,
                GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber),
                Email = customer.Email,
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

            Logger.Debug("Total Price for BuyNow: " + totalPrice);
            request.Price = decimal.Round(totalPrice, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            request.PaidPrice = decimal.Round(item.Price, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");

            request.BasketItems = basketItems;

            Logger.Info(SensitiveDataMasker.Mask(
                "Iyzico Request prepared for BuyNow CheckoutFormInitialization: ConversationId="
                + request.ConversationId
                + " BasketId="
                + request.BasketId));

            using (var activity = StartPaymentActivity("authorize_buynow"))
            {
                activity?.SetTag("order.conversation_id", request.ConversationId);
                try
                {
                    var result = await CheckoutFormInitialize.Create(request, options).ConfigureAwait(false);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    throw;
                }
            }
        }

        private Options GetOptions()
        {
            Logger.Debug("Fetching Iyzico API options...");
            var apiKey = AppConfig.IyzicoApiKey;
            var secretKey = AppConfig.IyzicoSecretKey;

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secretKey))
            {
                Logger.Error("Iyzico API credentials are not configured. Set IyzicoApiKey and IyzicoSecretKey via environment variables or AppSettings.");
                throw new InvalidOperationException(
                    "Iyzico payment gateway is not configured. Both IyzicoApiKey and IyzicoSecretKey must be set in secure configuration.");
            }

            Options options = new Options
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
                BaseUrl = AppConfig.IyzicoBaseUrl
            };
            Logger.Debug("Iyzico API options fetched successfully.");
            return options;
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