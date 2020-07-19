using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PaymentController : BaseController
    {
       
        private readonly IyzicoService iyzicoService;
        public PaymentController(IyzicoService iyzicoService)
        {
            this.iyzicoService = iyzicoService;
        }


        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        public ActionResult HomePageShoppingCart()
        {
            var shoppingCart = (ShoppingCartSession)Session[Constants.ShoppingCartSession];
            return PartialView("ShoppingCartTemplates/_HomePageShoppingCart", shoppingCart);
        }

        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = ProductService.GetProductById(productId);
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = new ShoppingCartItem();
            item.product = product.Product;
            item.quantity = quantity;
            item.ShoppingCartItemId = Guid.NewGuid().ToString();
            shoppingCart.Add(item);
            Session[Constants.ShoppingCartSession] = shoppingCart;
            return ShoppingCartItemsCount();
        }

        public ActionResult ShoppingCartItemsCount()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var totalItemNumber = shoppingCart.ShoppingCartItems.IsNullOrEmpty() ? "" : shoppingCart.ShoppingCartItems.Count() + "";
            return Json(new { totalItemNumber }, JsonRequestBehavior.AllowGet);
        }

        private ShoppingCartSession GetShoppingCart()
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[Constants.ShoppingCartSession];
            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCartSession();
                var shippingAddress = new Domain.Entities.Address();
                shippingAddress.Country = "Turkiye";
                var billingAddress = new Domain.Entities.Address();
                billingAddress.Country = "Turkiye";
                shoppingCart.ShippingAddress = shippingAddress;
                shoppingCart.BillingAddress = billingAddress;
                shoppingCart.Customer.IsSameAsShippingAddress = true;
                shoppingCart.Customer.Country = "Turkiye";
                shoppingCart.Customer.Ip = GetIpAddress();
                shoppingCart.Customer.IdentityNumber = Guid.NewGuid().ToString();
                Session[Constants.ShoppingCartSession] = shoppingCart;
            }

            return shoppingCart;
        }

        private String GetIpAddress()
        {
       return (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
            Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();
        }

        public ActionResult ShoppingCart()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }
        public ActionResult CustomerAccount()
        {
            return View();
        }

        public ActionResult CheckoutBillingDetails()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }
        public ActionResult SaveCustomer(Customer customer)
        {
            if (customer != null && customer.isValid())
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                shoppingCart.Customer = customer;
                if (customer.IsSameAsShippingAddress)
                {
                    setAddress(customer, shoppingCart.ShippingAddress);
                    setAddress(customer, shoppingCart.BillingAddress);
                }


                Session[Constants.ShoppingCartSession] = shoppingCart;
                return RedirectToAction("CheckoutDelivery");
            }
            else
            {
                return RedirectToAction("CheckoutBillingDetails");
            }
        }

        private static void setAddress(Customer customer, Domain.Entities.Address address)
        {
            address.City = customer.City;
            address.Country = customer.Country;
            address.ZipCode = customer.ZipCode;
            address.Description = customer.RegistrationAddress;
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
        public ActionResult RenderPrice(double price)
        {
            return Json(new { status = "success", price = price.CurrencySign() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateQuantity(String shoppingItemId, int quantity)
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[Constants.ShoppingCartSession];
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                item.quantity = quantity;
                Session[Constants.ShoppingCartSession] = shoppingCart;
                return Json(new { status = "success", shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "failed", shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
        }
         public ActionResult RemoveCart(String shoppingItemId)
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[Constants.ShoppingCartSession];
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId,StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                shoppingCart.ShoppingCartItems.Remove(item);
                Session[Constants.ShoppingCartSession] = shoppingCart;
                return Json(new { status = "success", shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "failed", shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: Home
        public ActionResult PlaceOrder()
        {
            ShoppingCartSession shoppingCart = (ShoppingCartSession)Session[Constants.ShoppingCartSession];
            if(shoppingCart == null || shoppingCart.ShoppingCartItems.IsNullOrEmpty())
            {
                return Content("ShoppingCartItems is EMPTY");
            }
            if (shoppingCart.Customer.isValid())
            {
                CheckoutFormInitialize checkoutFormInitialize = iyzicoService.CreateCheckoutFormInitialize(shoppingCart);
                return View(checkoutFormInitialize);
            }
            else
            {
                return Content("Customer is NOT valid");
            }
            
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