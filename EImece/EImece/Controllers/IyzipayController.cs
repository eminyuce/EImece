using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class IyzipayController : BaseController
    {
        private const string ApiKey = "sandbox-v0nW7JMLDP8x5ZjVN2MQpKkcmKlUqKZB";
        private const string SecretKey = "sandbox-xi3ZmT9EVV0AcwaV4mzT4TlOmWr5YGgL";
        private const string ShoppingCartSession = "ShoppingCartSession";

        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = ProductService.GetProductById(productId);
            ShoppingCart shoppingCart = (ShoppingCart)Session[ShoppingCartSession];
            if(shoppingCart == null)
            {
                shoppingCart = new ShoppingCart();
            }

            var item = new ShoppingCartItem();
            item.product = product.Product;
            item.quantity = quantity;
            shoppingCart.Add(item);
            Session[ShoppingCartSession] = shoppingCart;
            return RedirectToAction("Detail", "Products", new { id = productId });
        }
        public ActionResult ShoppingCart()
        {
            ShoppingCart addToCartProducts = (ShoppingCart)Session[ShoppingCartSession];
            return View(addToCartProducts);
        }
        // GET: Home
        public ActionResult Index()
        {
            Options options = new Options();
            options.ApiKey = ApiKey;
            options.SecretKey = SecretKey;
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
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
            buyer.Id = "1";
            buyer.Name = "Cengizhan";
            buyer.Surname = "Bozkurt";
            buyer.GsmNumber = "-";
            buyer.Email = "cenboz27@gmail.com";
            buyer.IdentityNumber = "12345678911";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "Antalya";
            buyer.Ip = "85.34.78.112";
            buyer.City = "Antalya";
            buyer.Country = "Turkey";
            buyer.ZipCode = "07600";
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
            var products = ProductService.GetMainPageProducts(1, CurrentLanguage);
            double price = 0;
            foreach (Product item in products.Products) //Session'da tutmuş oldugum sepette bulunan ürünler
            {
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
            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);
         //   ViewBag.Iyzico = checkoutFormInitialize; //View Dönüş yapılan yer, Burada farklı yöntemler ile View gönderim yapabilirsiniz.
            return View(checkoutFormInitialize);
        }

        public ActionResult Sonuc(RetrieveCheckoutFormRequest model)
        {
            string data = "";
            Options options = new Options();
            options.ApiKey = ApiKey;
            options.SecretKey = SecretKey;
            options.BaseUrl = "https://sandbox-api.iyzipay.com";
            data = model.Token;
            RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest();
            request.Token = data;
            CheckoutForm checkoutForm = CheckoutForm.Retrieve(request, options);
            if (checkoutForm.PaymentStatus == "SUCCESS")
            {
                return RedirectToAction("Onay");
            }

            return View();
        }
    }
}