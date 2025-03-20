using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Services
{
    public class IyzicoService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CheckoutForm GetCheckoutForm(RetrieveCheckoutFormRequest model)
        {
            Logger.Info("Retrieving checkout form with token: " + model.Token);
            Options options = GetOptions();
            var request = new RetrieveCheckoutFormRequest();
            request.Token = model.Token;

            Logger.Debug("CheckoutForm request prepared with Token: " + model.Token);
            var response = CheckoutForm.Retrieve(request, options);
            Logger.Info("CheckoutForm retrieved successfully with Token: " + model.Token);
            return response;
        }

        public CheckoutFormInitialize CreateCheckoutFormInitialize(ShoppingCartSession shoppingCart, string userId)
        {
            Logger.Info("Initializing CheckoutForm for user: " + userId);

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

            Logger.Debug("Fetching Iyzico API options...");
            Options options = GetOptions();
            var customer = shoppingCart.Customer;

            Logger.Debug("Building callback URL for Payment Result...");
            var requestContext = HttpContext.Current.Request.RequestContext;
            string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(shoppingCart.OrderGuid));
            string u = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(userId));
            string callbackUrl = new UrlHelper(requestContext).Action("PaymentResult",
                                               "Payment",
                                               new { o, u },
                                               AppConfig.HttpProtocol);

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = shoppingCart.ConversationId,
                Currency = Currency.TRY.ToString(),
                BasketId = shoppingCart.OrderGuid,
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = callbackUrl,
                EnabledInstallments = AppConfig.IyzicoEnabledInstallments
            };

            Logger.Debug("CheckoutFormInitializeRequest object populated");

            // Buyer details
            Buyer buyer = new Buyer
            {
                Id = customer.Id.ToString(),
                Name = customer.Name,
                Surname = customer.Surname,
                GsmNumber = customer.GsmNumber,
                Email = customer.Email,
                IdentityNumber = customer.IdentityNumber,
                LastLoginDate = customer.UpdatedDate.ToString(Constants.IyzicoDateTimeFormat),
                RegistrationDate = customer.CreatedDate.ToString(Constants.IyzicoDateTimeFormat),
                RegistrationAddress = customer.RegistrationAddress,
                Ip = customer.Ip,
                City = customer.City,
                Country = customer.Country,
                ZipCode = customer.ZipCode
            };
            request.Buyer = buyer;

            if (shoppingCart.Customer.IsSameAsShippingAddress)
            {
                Address shippingAddress = new Address
                {
                    ContactName = shoppingCart.Customer.FullName,
                    City = shoppingCart.ShippingAddress.City,
                    Country = shoppingCart.ShippingAddress.Country,
                    Description = shoppingCart.ShippingAddress.Description,
                    ZipCode = shoppingCart.ShippingAddress.ZipCode
                };
                request.ShippingAddress = shippingAddress;
                request.BillingAddress = shippingAddress;
            }
            else
            {
                Address shippingAddress = new Address
                {
                    ContactName = shoppingCart.Customer.FullName,
                    City = shoppingCart.ShippingAddress.City,
                    Country = shoppingCart.ShippingAddress.Country,
                    Description = shoppingCart.ShippingAddress.Description,
                    ZipCode = shoppingCart.ShippingAddress.ZipCode
                };
                request.ShippingAddress = shippingAddress;

                Address billingAddress = new Address
                {
                    ContactName = shoppingCart.Customer.FullName,
                    City = shoppingCart.BillingAddress.City,
                    Country = shoppingCart.BillingAddress.Country,
                    Description = shoppingCart.BillingAddress.Description,
                    ZipCode = shoppingCart.BillingAddress.ZipCode
                };
                request.BillingAddress = billingAddress;
            }

            List<BasketItem> basketItems = new List<BasketItem>();
            decimal totalPrice = 0;

            foreach (ShoppingCartItem shoppingCartItem in shoppingCart.ShoppingCartItems)
            {
                var item = shoppingCartItem.Product;
                BasketItem firstBasketItem = new BasketItem
                {
                    Id = item.ProductCode,
                    Name = item.Name,
                    Category1 = item.CategoryName,
                    Category2 = AppConfig.ShoppingCartItemCategory2,
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = CurrencyHelper.CurrencySignForIyizo(item.Price)
                };
                totalPrice += item.Price;
                basketItems.Add(firstBasketItem);
            }

            // Log prices in a readable format
            Logger.Debug("Total Price: " + totalPrice);
            Logger.Debug("Shipping & Paid Price: " + shoppingCart.TotalPriceWithCargoPrice);

            request.Price = totalPrice.CurrencySignForIyizo();
            request.PaidPrice = shoppingCart.TotalPriceWithCargoPrice.CurrencySignForIyizo();
            request.BasketItems = basketItems;

            Logger.Info("Iyzico Request prepared for CheckoutFormInitialization: " + JsonConvert.SerializeObject(request));

            return CheckoutFormInitialize.Create(request, options);
        }

        public CheckoutFormInitialize CreateCheckoutFormInitializeBuyNow(BuyNowModel buyNowModel)
        {
            Logger.Info("Initializing CheckoutForm for BuyNow with OrderGuid: " + buyNowModel.OrderGuid);

            Options options = GetOptions();
            var customer = buyNowModel.Customer;

            Logger.Debug("Building callback URL for BuyNow Payment Result...");
            var requestContext = HttpContext.Current.Request.RequestContext;
            string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(buyNowModel.OrderGuid));
            string callbackUrl = new UrlHelper(requestContext).Action("BuyNowPaymentResult",
                                               "Payment",
                                               new { o },
                                               AppConfig.HttpProtocol);

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
                Id = customer.Id.ToString(),
                Name = customer.Name,
                Surname = customer.Surname,
                GsmNumber = customer.GsmNumber,
                Email = customer.Email,
                IdentityNumber = customer.IdentityNumber,
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

            var item = buyNowModel.ProductDetailViewModel.Product;
            BasketItem firstBasketItem = new BasketItem
            {
                Id = item.ProductCode,
                Name = item.NameLong,
                Category1 = item.ProductCategory.Name,
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

            Logger.Info("Iyzico Request prepared for BuyNow CheckoutFormInitialization: " + JsonConvert.SerializeObject(request));

            return CheckoutFormInitialize.Create(request, options);
        }

        private Options GetOptions()
        {
            Logger.Debug("Fetching Iyzico API options...");
            Options options = new Options
            {
                ApiKey = AppConfig.IyzicoApiKey,
                SecretKey = AppConfig.IyzicoSecretKey,
                BaseUrl = AppConfig.IyzicoBaseUrl
            };
            Logger.Debug("Iyzico API options fetched successfully.");
            return options;
        }
    }
}
