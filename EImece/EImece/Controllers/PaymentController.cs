using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PaymentController : BaseController
    {
        private const string OrderGuidCookieKey = "orderGuid";
        private const string SUCCESS = "SUCCESS";
        private const string FAILED = "FAILED";

        private readonly IyzicoService iyzicoService;

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        public IAddressService AddressService { get; set; }

        public ICustomerService CustomerService { get; set; }

        public PaymentController(IyzicoService iyzicoService,
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
             AddressService addressService, CustomerService customerService)
        {
            this.iyzicoService = iyzicoService;
            UserManager = userManager;
            SignInManager = signInManager;
            AddressService = addressService;
            CustomerService = customerService;
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

        public ActionResult AddToCart(int productId, int quantity, string orderGuid)
        {
            var product = ProductService.GetProductById(productId);
            var shoppingCart = GetShoppingCart();
            shoppingCart.OrderGuid = orderGuid;
            var item = new ShoppingCartItem();
            item.product = new ShoppingCartProduct(product.Product);
            item.quantity = quantity;
            item.ShoppingCartItemId = Guid.NewGuid().ToString();
            shoppingCart.Add(item);
            SaveShoppingCart(shoppingCart);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [NoCache]
        public ActionResult GetShoppingCartSmallDetails()
        {
            ShoppingCartSession shoppingCart = GetShoppingCartFromDataSource();
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\ShoppingCartTemplates\_ShoppingCartSmallDetails.cshtml",
                        new ViewDataDictionary(shoppingCart), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
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
            SaveShoppingCartCookie(shoppingCart);
        }

        private void SaveShoppingCartCookie(ShoppingCartSession shoppingCart)
        {
            var item = new Domain.Entities.ShoppingCart();
            item.CreatedDate = DateTime.Today;
            item.UpdatedDate = DateTime.Today;
            item.Name = "Name";
            item.IsActive = false;
            item.Lang = CurrentLanguage;
            item.Position = 0;
            item.ShoppingCartJson = JsonConvert.SerializeObject(shoppingCart);
            item.OrderGuid = shoppingCart.OrderGuid;
            ShoppingCartService.SaveOrEditShoppingCart(item);
        }

        private ShoppingCartSession GetShoppingCartFromDataSource()
        {
            HttpCookie orderGuid = Request.Cookies[OrderGuidCookieKey];
            ShoppingCartSession result = null;
            var item = orderGuid != null ? ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid.Value) : null;
            if (item == null)
            {
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
                GetCustomerIfAuthenticated(result);
            }
            else
            {
                result = JsonConvert.DeserializeObject<ShoppingCartSession>(item.ShoppingCartJson);
            }
            return result;
        }

        private void GetCustomerIfAuthenticated(ShoppingCartSession result)
        {
            if (Request.IsAuthenticated)
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (user != null)
                {
                    result.Customer = CustomerService.GetUserId(user.Id);
                }
            }
        }

        private ShoppingCartSession GetShoppingCart()
        {
            return GetShoppingCartFromDataSource();
        }

        public ActionResult ShoppingCart()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                shoppingCart.UrlReferrer = urlReferrer.ToStr();
            }
            return View(shoppingCart);
        }

        public ActionResult CheckoutBillingDetails()
        {
            if (Request.IsAuthenticated)
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                if (shoppingCart.Customer.IsEmpty())
                {
                    GetCustomerIfAuthenticated(shoppingCart);
                }
                return View(shoppingCart);
            }
            else
            {
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("CheckoutPaymentOrderReview", "Payment") });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCustomer(Customer customer)
        {
            if (customer != null && customer.isValid())
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                shoppingCart.Customer = customer;
                if (customer.IsSameAsShippingAddress)
                {
                }

                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;

                SaveShoppingCart(shoppingCart);
                return RedirectToAction("CheckoutPaymentOrderReview");
            }
            else
            {
                return RedirectToAction("CheckoutBillingDetails");
            }
        }

        private Domain.Entities.Address SetAddress(Customer customer, Domain.Entities.Address address)
        {
            if (address == null)
            {
                address = new Domain.Entities.Address();
            }
            address.Street = customer.Street;
            address.District = customer.District;
            address.City = customer.City;
            address.Country = customer.Country;
            address.ZipCode = customer.ZipCode;
            address.Description = customer.Description;
            address.Name = customer.FullName;
            address.CreatedDate = DateTime.Now;
            address.UpdatedDate = DateTime.Now;
            address.IsActive = true;
            address.Position = 1;
            address.Lang = CurrentLanguage;
            return address;
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
            return Json(new { status = SUCCESS, price = price.CurrencySign() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sendOrderComments(string orderComments, string orderGuid)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            shoppingCart.OrderComments = orderComments;
            SaveShoppingCart(shoppingCart);
            return Json(new { status = SUCCESS }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateQuantity(String shoppingItemId, int quantity)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                item.quantity = quantity;
                SaveShoppingCart(shoppingCart);
                return Json(new { status = SUCCESS, shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = FAILED, shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveCart(String shoppingItemId)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                shoppingCart.ShoppingCartItems.Remove(item);
                SaveShoppingCart(shoppingCart);
                return Json(new { status = SUCCESS, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = FAILED, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PlaceOrder()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsEmpty())
            {
                return Content("ShoppingCartItems is EMPTY");
            }
            if (shoppingCart.Customer.isValid())
            {
                var checkoutFormInitialize = iyzicoService.CreateCheckoutFormInitialize(shoppingCart);
                ViewBag.CheckoutFormInitialize = checkoutFormInitialize;
                return View(shoppingCart);
            }
            else
            {
                return Content("RegisterCustomer");
            }
        }

        public ActionResult PaymentResult(RetrieveCheckoutFormRequest model)
        {
            CheckoutForm checkoutForm = iyzicoService.GetCheckoutForm(model);
            if (checkoutForm.PaymentStatus.Equals(SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                ShoppingCartService.SaveShoppingCart(shoppingCart, checkoutForm);
                ClearCart(shoppingCart);
                Task.Run(() =>
                {
                    var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(shoppingCart);
                    EmailSender.SendOrderConfirmationEmail(SettingService.GetEmailAccount(), shoppingCart, emailTemplate);
                });
            }
            return View(checkoutForm);
        }

        private void ClearCart(ShoppingCartSession shoppingCart)
        {
            if (Request.Browser.Cookies)
            {
                Response.Cookies.Remove(OrderGuidCookieKey);
                var aCookie = new HttpCookie(OrderGuidCookieKey) { Expires = DateTime.Now.AddDays(-1) };
                Response.Cookies.Add(aCookie);
            }
            ShoppingCartService.DeleteByOrderGuid(shoppingCart.OrderGuid);
        }

        public ActionResult PaymentSuccess(RetrieveCheckoutFormRequest model)
        {
            return Content("PaymentSuccess is done");
        }
    }
}