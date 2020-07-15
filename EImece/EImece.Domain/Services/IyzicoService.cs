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
            request.ConversationId = "123456789";
            request.Price = "1"; // Tutar
            request.PaidPrice = "1.1";
            request.Currency = Currency.TRY.ToString();
            request.BasketId = "B67832";
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
            buyer.IdentityNumber = customer.IdentityNumber;
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = customer.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss");
            buyer.RegistrationAddress = customer.RegistrationAddress;
            buyer.Ip = customer.Ip;
            buyer.City = customer.City;
            buyer.Country = customer.Country;
            buyer.ZipCode = customer.ZipCode;
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Cengizhan Bozkurt";
            shippingAddress.City = "Antalya";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Mahalle --- Antalya";
            shippingAddress.ZipCode = "07600";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Cengizhan Bozkurt";
            billingAddress.City = "Turkey";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Mahalle --- Antalya";
            billingAddress.ZipCode = "07600";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();

            double price = 0;
            foreach (ShoppingCartItem shoppingCartItem in shoppingCart.ShoppingCartItems) //Session'da tutmuş oldugum sepette bulunan ürünler
            {
                var item = shoppingCartItem.product;
                BasketItem firstBasketItem = new BasketItem();
                firstBasketItem.Id = string.IsNullOrEmpty(item.ProductCode) ? item.Id.ToString() : item.ProductCode;
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
            options.BaseUrl = "https://sandbox-api.iyzipay.com";
            return options;
        }
    }
}
