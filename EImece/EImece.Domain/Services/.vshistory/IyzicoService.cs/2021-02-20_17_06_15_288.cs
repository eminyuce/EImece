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
            return CheckoutForm.Retrieve(request, options);
        }

        /****
         * https://dev.iyzipay.com/tr/odeme-formu/odeme-formu-baslatma
         *
         */

        public CheckoutFormInitialize CreateCheckoutFormInitialize(ShoppingCartSession shoppingCart, string userId)
        {
            if (shoppingCart == null)
            {
                throw new ArgumentNullException("ShoppingCartSession cannot be null");
            }
            if (shoppingCart.ShoppingCartItems.IsEmpty())
            {
                throw new ArgumentNullException("ShoppingCartSession.ShoppingCartItems cannot be null");
            }
            if (shoppingCart.Customer == null)
            {
                throw new ArgumentNullException("ShoppingCartSession.Customer cannot be null");
            }
       

            Options options = GetOptions();
            var customer = shoppingCart.Customer;

            var requestContext = HttpContext.Current.Request.RequestContext;
            string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(shoppingCart.OrderGuid));
            string u = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(userId));

            string callbackUrl = new UrlHelper(requestContext).Action("PaymentResult",
                                               "Payment",
                                               new { o, u },
                                               AppConfig.HttpProtocol);

            var request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            //İstek esnasında gönderip, sonuçta alabileceğiniz bir değer, request/response eşleşmesi yapmak için kullanılabilir.
            request.ConversationId = shoppingCart.ConversationId;

            request.Currency = Currency.TRY.ToString();
            request.BasketId = shoppingCart.OrderGuid;
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = callbackUrl; /// Geri Dönüş Urlsi
            request.EnabledInstallments = AppConfig.IyzicoEnabledInstallments;

            Buyer buyer = new Buyer();
            buyer.Id = customer.Id.ToString();
            buyer.Name = customer.Name;
            buyer.Surname = customer.Surname;
            buyer.GsmNumber = customer.GsmNumber;
            buyer.Email = customer.Email;
            buyer.IdentityNumber = customer.IdentityNumber;
            buyer.LastLoginDate = customer.UpdatedDate.ToString(Constants.IyzicoDateTimeFormat);
            buyer.RegistrationDate = customer.CreatedDate.ToString(Constants.IyzicoDateTimeFormat);
            buyer.RegistrationAddress = customer.RegistrationAddress;
            buyer.Ip = customer.Ip;
            buyer.City = customer.City;
            buyer.Country = customer.Country;
            buyer.ZipCode = customer.ZipCode;
            request.Buyer = buyer;
            if (shoppingCart.Customer.IsSameAsShippingAddress)
            {
                Address shippingAddress = new Address();
                shippingAddress.ContactName = shoppingCart.Customer.FullName;
                Entities.Address shippingAddress1 = shoppingCart.ShippingAddress;
                shippingAddress.City = shippingAddress1.City;
                shippingAddress.Country = shippingAddress1.Country;
                shippingAddress.Description = shippingAddress1.Description;
                shippingAddress.ZipCode = shippingAddress1.ZipCode;
                request.ShippingAddress = shippingAddress;
                request.BillingAddress = shippingAddress;
            }
            else
            {
                Address shippingAddress = new Address();
                shippingAddress.ContactName = shoppingCart.Customer.FullName;
                shippingAddress.City = shoppingCart.ShippingAddress.City;
                shippingAddress.Country = shoppingCart.ShippingAddress.Country;
                shippingAddress.Description = shoppingCart.ShippingAddress.Description;
                shippingAddress.ZipCode = shoppingCart.ShippingAddress.ZipCode;
                request.ShippingAddress = shippingAddress;

                Address billingAddress = new Address();
                billingAddress.ContactName = shoppingCart.Customer.FullName;
                Entities.Address billingAddress1 = shoppingCart.BillingAddress;
                billingAddress.City = billingAddress1.City;
                billingAddress.Country = billingAddress1.Country;
                billingAddress.Description = billingAddress1.Description;
                billingAddress.ZipCode = billingAddress1.ZipCode;
                request.BillingAddress = billingAddress;
            }

            List<BasketItem> basketItems = new List<BasketItem>();
            decimal totalPrice = 0;
            foreach (ShoppingCartItem shoppingCartItem in shoppingCart.ShoppingCartItems) //Session'da tutmuş oldugum sepette bulunan ürünler
            {
                var item = shoppingCartItem.Product;
                BasketItem firstBasketItem = new BasketItem();
                firstBasketItem.Id = item.ProductCode;
                firstBasketItem.Name = item.Name;
                firstBasketItem.Category1 = item.CategoryName;
                firstBasketItem.Category2 = AppConfig.ShoppingCartItemCategory2;
                firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();

                firstBasketItem.Price = decimal.Round(item.Price, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", "."); // item.Price.ToString("0.0", CultureInfo.GetCultureInfo(Constants.EN_US_CULTURE_INFO));
                totalPrice += item.Price;
                basketItems.Add(firstBasketItem);
            }
            //Client'a fiyat bilgisi olarak noktalı yollamanız gerekir. Virgüllü yollarsanız hata alırsınız. Bu yüzden fiyat bilgisinde client kullanırken noktalı yollamanız gerekir.
            request.Price = decimal.Round(totalPrice, 2, MidpointRounding.AwayFromZero).ToString().Replace(",",".");  //totalPrice.ToString("0.0", CultureInfo.GetCultureInfo(Constants.EN_US_CULTURE_INFO)); // Tutar
            request.PaidPrice = decimal.Round(shoppingCart.TotalPriceWithCargoPrice, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", "."); //shoppingCart.TotalPriceWithCargoPrice.ToString("0.0", CultureInfo.GetCultureInfo(Constants.EN_US_CULTURE_INFO)); // Tutar
            request.BasketItems = basketItems;
            Logger.Info("Iyizco Request:" + JsonConvert.SerializeObject(request));
            return CheckoutFormInitialize.Create(request, options);
        }

        private Options GetOptions()
        {
            Options options = new Options();
            options.ApiKey = AppConfig.IyzicoApiKey;
            options.SecretKey = AppConfig.IyzicoSecretKey;
            options.BaseUrl = AppConfig.IyzicoBaseUrl;
            return options;
        }
    }
}