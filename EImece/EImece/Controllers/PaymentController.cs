using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Services;
using EImece.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Ninject;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly IyzicoService iyzicoService;

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        public PaymentController(IyzicoService iyzicoService,ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            this.iyzicoService = iyzicoService;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        public ActionResult HomePageShoppingCart()
        {
            return PartialView("ShoppingCartTemplates/_HomePageShoppingCart", GetShoppingCart());
        }

        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = ProductService.GetProductById(productId);
            var shoppingCart = GetShoppingCart();
            var item = new ShoppingCartItem();
            item.product = new ShoppingCartProduct(product.Product);
            item.quantity = quantity;
            item.ShoppingCartItemId = Guid.NewGuid().ToString();
            shoppingCart.Add(item);
            SaveShoppingCart(shoppingCart);
            return Json("", JsonRequestBehavior.AllowGet);
        }

       
        [NoCache]
        public ActionResult GetShoppingCartLinks()
        {
            ShoppingCartSession shoppingCart = GetShoppingCartFromDataSource();
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\ShoppingCartTemplates\_ShoppingCartLinks.cshtml",
                        new ViewDataDictionary(shoppingCart), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        [NoCache]
        public ActionResult ShoppingCartLink()
        {
            ShoppingCartSession shoppingCart = GetShoppingCartFromDataSource();
            return PartialView("ShoppingCartTemplates/_ShoppingCartLinks", shoppingCart);
        }

        private void SaveShoppingCart(ShoppingCartSession shoppingCart)
        {
            //   Session[Constants.ShoppingCartKey] = shoppingCart;

            SaveShoppingCartCookie(shoppingCart);

        }

        private void SaveShoppingCartCookie(ShoppingCartSession shoppingCart)
        {
            SetCookie(Constants.ShoppingCartKey, JsonConvert.SerializeObject(shoppingCart), 120);
        }
        public bool SetCookie(string cookieName, string value, int expires)
        {
            try
            {
                HttpCookie cookie_clc = new HttpCookie(cookieName, value);
                cookie_clc.Expires = DateTime.Now.AddMinutes(expires);
                cookie_clc.Path = "/"+ cookieName;
                Response.Cookies.Add(cookie_clc);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        private ShoppingCartSession GetShoppingCartFromDataSource()
        {
               return GetShoppingCartFromFromCookie();
            //return GetShoppingCartFromFromSession();
        }

        private ShoppingCartSession GetShoppingCartFromFromSession()
        {
            var result = (ShoppingCartSession)Session[Constants.ShoppingCartKey];
            if (result == null)
            {
                var shoppingCart = CreateDefaultShopingCard();
                SaveShoppingCart(shoppingCart);
                return shoppingCart;
            }
            return result;
        }

        private ShoppingCartSession GetShoppingCartFromFromCookie()
        {
            HttpCookie shoppingCartCookie = GetShoppingCookie();
            if (shoppingCartCookie == null || String.IsNullOrEmpty(shoppingCartCookie.Value))
            {
                return CreateDefaultShopingCard();
            }
            else
            {
                return JsonConvert.DeserializeObject<ShoppingCartSession>(shoppingCartCookie.Value);
            }
        }

        private HttpCookie GetShoppingCookie()
        {
            return Response.Cookies[Constants.ShoppingCartKey];
        }

        private ShoppingCartSession GetShoppingCart()
        {
            return GetShoppingCartFromDataSource();
        }

        private ShoppingCartSession CreateDefaultShopingCard()
        {
            ShoppingCartSession shoppingCart = new ShoppingCartSession();
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

                SaveShoppingCart(shoppingCart);
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
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                item.quantity = quantity;
                SaveShoppingCart(shoppingCart);
                return Json(new { status = "success", shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "failed", shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
        }
      
        public ActionResult RemoveCart(String shoppingItemId)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId,StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                shoppingCart.ShoppingCartItems.Remove(item);
                SaveShoppingCart(shoppingCart);
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
            ShoppingCartSession shoppingCart = GetShoppingCart();
            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsNullOrEmpty())
            {
                return Content("ShoppingCartItems is EMPTY");
            }
            if (shoppingCart.Customer.isValid())
            {
                var checkoutFormInitialize = iyzicoService.CreateCheckoutFormInitialize(shoppingCart);
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