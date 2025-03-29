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
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Services
{
    public class IyzicoService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CheckoutForm GetCheckoutForm(RetrieveCheckoutFormRequest model)
        {
            Options options = GetOptions();
            var request = new RetrieveCheckoutFormRequest();
            request.Token = model.Token;

            var response = CheckoutForm.Retrieve(request, options);
            return response.Result;
        }

        public CheckoutFormInitialize CreateCheckoutFormInitialize_Working_Code(ShoppingCartSession shoppingCart, string userId)
        {
            Logger.Info("Payment.Create static request is ongoing: " + userId);

            Options options = new Options();
            options.ApiKey = AppConfig.IyzicoApiKey;
            options.SecretKey = AppConfig.IyzicoSecretKey;
            options.BaseUrl = AppConfig.IyzicoBaseUrl;


            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = "1";
            request.PaidPrice = "1.2";
            request.Currency = Currency.TRY.ToString();
            request.BasketId = "B67832";
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "https://www.merchant.com/callback";

            List<int> enabledInstallments = new List<int>();
            enabledInstallments.Add(2);
            enabledInstallments.Add(3);
            enabledInstallments.Add(6);
            enabledInstallments.Add(9);
            request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer();
            buyer.Id = "BY789";
            buyer.Name = "John";
            buyer.Surname = "Doe";
            buyer.GsmNumber = "+905350000000";
            buyer.Email = "email@email.com";
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            buyer.Ip = "85.34.78.112";
            buyer.City = "Istanbul";
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = "BI101";
            firstBasketItem.Name = "Binocular";
            firstBasketItem.Category1 = "Collectibles";
            firstBasketItem.Category2 = "Accessories";
            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = "0.3";
            basketItems.Add(firstBasketItem);

            BasketItem secondBasketItem = new BasketItem();
            secondBasketItem.Id = "BI102";
            secondBasketItem.Name = "Game code";
            secondBasketItem.Category1 = "Game";
            secondBasketItem.Category2 = "Online Game Items";
            secondBasketItem.ItemType = BasketItemType.VIRTUAL.ToString();
            secondBasketItem.Price = "0.5";
            basketItems.Add(secondBasketItem);

            BasketItem thirdBasketItem = new BasketItem();
            thirdBasketItem.Id = "BI103";
            thirdBasketItem.Name = "Usb";
            thirdBasketItem.Category1 = "Electronics";
            thirdBasketItem.Category2 = "Usb / Cable";
            thirdBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            thirdBasketItem.Price = "0.2";
            basketItems.Add(thirdBasketItem);
            request.BasketItems = basketItems;

            Logger.Info("Initializing CheckoutFormInitialize.Create for user: " + userId);
            System.Threading.Tasks.Task<CheckoutFormInitialize> checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);
            return checkoutFormInitialize.Result;
        }
        public CheckoutFormInitialize CreateCheckoutFormInitialize(ShoppingCartSession shoppingCart, string userId)
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
            Logger.Debug("Fetching Iyzico API options...");
            Options options = new Options
            {
                ApiKey = AppConfig.IyzicoApiKey,
                SecretKey = AppConfig.IyzicoSecretKey,
                BaseUrl = AppConfig.IyzicoBaseUrl
            };

            // Build callback URL
            Logger.Debug("Building callback URL for Payment Result...");
            var requestContext = HttpContext.Current.Request.RequestContext;
            string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(shoppingCart.OrderGuid));
            string u = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(userId));
            string callbackUrl = new UrlHelper(requestContext).Action("PaymentResult",
                                               "Payment",
                                               new { o, u },
                                               AppConfig.HttpProtocol);

            // Initialize request
            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = shoppingCart.ConversationId,
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

            // Log prices and request details
            Logger.Debug("Total Price after CurrencySignForIyizo: " + request.Price);
            Logger.Debug("Shipping & Paid Price after CurrencySignForIyizo: " + request.PaidPrice);
            Logger.Info("Iyzico Request prepared for CheckoutFormInitialization: " + JsonConvert.SerializeObject(request));

            // Execute the request
            Logger.Info("Initializing CheckoutFormInitialize.Create for user: " + userId);
            System.Threading.Tasks.Task<CheckoutFormInitialize> checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);
            return checkoutFormInitialize.Result;
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

            var result = CheckoutFormInitialize.Create(request, options);
            
            return result.Result;
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