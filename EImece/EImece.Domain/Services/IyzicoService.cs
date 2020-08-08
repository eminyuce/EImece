using EImece.Domain.Models.FrontModels;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Ninject;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Services
{
    public class IyzicoService
    {
        [Inject]
        private CustomerService CustomerService;

        [Inject]
        private ShoppingCartService ShoppingCartService;

        [Inject]
        private AddressService AddressService;

        public CheckoutForm GetCheckoutForm(RetrieveCheckoutFormRequest model)
        {
            string data = "";
            Options options = GetOptions();
            data = model.Token;
            var request = new RetrieveCheckoutFormRequest();
            request.Token = data;
            return CheckoutForm.Retrieve(request, options);
        }

        public CheckoutFormInitialize CreateCheckoutFormInitialize(ShoppingCartSession shoppingCart)
        {
            Options options = GetOptions();
            var customer = shoppingCart.Customer;
            var requestContext = HttpContext.Current.Request.RequestContext;
            string CallbackUrl = new UrlHelper(requestContext).Action("PaymentResult",
                                               "Payment",
                                               null,
                                               AppConfig.HttpProtocol);

            var request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = shoppingCart.OrderGuid;
            request.Currency = Currency.TRY.ToString();
            request.BasketId = shoppingCart.OrderGuid;
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = CallbackUrl; /// Geri Dönüş Urlsi

            List<int> enabledInstallments = new List<int>();
            enabledInstallments.Add(2);
            enabledInstallments.Add(3);
            enabledInstallments.Add(6);
            enabledInstallments.Add(9);
            request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer();
            buyer.Id = customer.Id.ToString();
            buyer.Name = customer.Name;
            buyer.Surname = customer.Surname;
            buyer.GsmNumber = customer.GsmNumber;
            buyer.Email = customer.Email;
            buyer.IdentityNumber = "38108089458";
            buyer.LastLoginDate = customer.UpdatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            buyer.RegistrationDate = customer.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            buyer.RegistrationAddress = customer.Description;
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
            double price = 0;
            foreach (ShoppingCartItem shoppingCartItem in shoppingCart.ShoppingCartItems) //Session'da tutmuş oldugum sepette bulunan ürünler
            {
                var item = shoppingCartItem.product;
                BasketItem firstBasketItem = new BasketItem();
                firstBasketItem.Id = item.ProductCode;
                firstBasketItem.Name = item.Name;
                firstBasketItem.Category1 = item.CategoryName;
                firstBasketItem.Category2 = "Ürün";
                firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
                firstBasketItem.Price = item.Price.ToString();
                price += item.Price;
                basketItems.Add(firstBasketItem);
            }

            request.Price = price.ToString(); // Tutar
            request.PaidPrice = price.ToString();
            request.BasketItems = basketItems;

            return CheckoutFormInitialize.Create(request, options); ;
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