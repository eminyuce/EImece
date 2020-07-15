using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.FrontModels;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ninject;

namespace EImece.Domain.Services
{
    public class IyzicoService
    {

        [Inject]
        CustomerService CustomerService;

        [Inject]
        ShoppingCartService ShoppingCartService;

        [Inject]
        AddressService AddressService;


        public CheckoutForm GetCheckoutForm(RetrieveCheckoutFormRequest model)
        {
            string data = "";
            Options options = GetOptions();
            data = model.Token;
            RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest();
            request.Token = data;
            return CheckoutForm.Retrieve(request, options); ;
        }
        public CheckoutFormInitialize CreateCheckoutFormInitialize(ShoppingCartSession shoppingCart)
        {
            Options options = GetOptions();
            var customer = shoppingCart.Customer;
            
            
            var request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = Guid.NewGuid().ToString();
            request.Currency = Currency.TRY.ToString();
            request.BasketId = Guid.NewGuid().ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "http:/<Iyzico Api Geri Dönüş Adresi>/OdemeSonucu"; /// Geri Dönüş Urlsi

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
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = customer.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss");
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
                shippingAddress.City = shoppingCart.ShippingAddress.City;
                shippingAddress.Country = shoppingCart.ShippingAddress.Country;
                shippingAddress.Description = shoppingCart.ShippingAddress.Description;
                shippingAddress.ZipCode = shoppingCart.ShippingAddress.ZipCode;
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
                billingAddress.City = shoppingCart.BillingAddress.City;
                billingAddress.Country = shoppingCart.BillingAddress.Country;
                billingAddress.Description = shoppingCart.BillingAddress.Description;
                billingAddress.ZipCode = shoppingCart.BillingAddress.ZipCode;
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
                firstBasketItem.Category1 = item.ProductCategory.Name;
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

        private  Options GetOptions()
        {
            Options options = new Options();
            options.ApiKey = AppConfig.IyzicoApiKey;
            options.SecretKey = AppConfig.IyzicoSecretKey;
            options.BaseUrl = AppConfig.IyzicoBaseUrl;
            return options;
        }
    }
}
