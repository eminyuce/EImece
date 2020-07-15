using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PaymentController : BaseController
    {
       
        private const string ShoppingCartSession = "ShoppingCartSession";
        private readonly IyzicoService iyzicoService;
        public PaymentController(IyzicoService iyzicoService)
        {
            this.iyzicoService = iyzicoService;
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = ProductService.GetProductById(productId);
            ShoppingCartSession shoppingCart = GetShoppingCart();

            var item = new ShoppingCartItem();
            item.product = product.Product;
            item.quantity = quantity;
            shoppingCart.Add(item);
            Session[ShoppingCartSession] = shoppingCart;
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        private ShoppingCartSession GetShoppingCart()
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[ShoppingCartSession];
            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCartSession();
            }

            return shoppingCart;
        }

        public ActionResult ShoppingCart()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }

        public ActionResult CheckoutBillingDetails()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }
        public ActionResult SaveCustomer(Customer customer)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            shoppingCart.Customer = customer;
            Session[ShoppingCartSession] = shoppingCart;
            return RedirectToAction("CheckoutDelivery");
        }
        public ActionResult CheckoutDelivery()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }

        public ActionResult CheckoutPaymentOrderReview()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }
         public ActionResult RemoveCart(int id)
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[ShoppingCartSession];
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.product.Id == id);
            if (item != null)
            {
                shoppingCart.ShoppingCartItems.Remove(item);
                Session[ShoppingCartSession] = shoppingCart;
            }

            return RedirectToAction("ShoppingCart");
        }
        // GET: Home
        public ActionResult PlaceOrder()
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[ShoppingCartSession];
            CheckoutFormInitialize checkoutFormInitialize = iyzicoService.CreateCheckoutFormInitialize(shoppingCart);
            return View(checkoutFormInitialize);
        }

       

        public ActionResult Sonuc(RetrieveCheckoutFormRequest model)
        {
            CheckoutForm checkoutForm = iyzicoService.GetCheckoutForm(model);
            if (checkoutForm.PaymentStatus == "SUCCESS")
            {
                return RedirectToAction("Onay");
            }

            return View();
        }

    }
}