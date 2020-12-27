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
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json;
using Ninject;
using NLog;
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

        private static readonly Logger PaymentLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        public IAddressService AddressService { get; set; }

        public ICustomerService CustomerService { get; set; }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

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

        public ActionResult AddToCart(int productId, int quantity, string orderGuid, string productSpecItems)
        {
           
            var product = ProductService.GetProductById(productId);
            var shoppingCart = GetShoppingCart();
            shoppingCart.OrderGuid = orderGuid;
 
            var item = new ShoppingCartItem();
            var selectedTotalSpecs = new List<ProductSpecItem>();
            if (!string.IsNullOrEmpty(productSpecItems))
            {
                var ooo = JsonConvert.DeserializeObject<ProductSpecItemRoot>(productSpecItems);
                selectedTotalSpecs = ooo.selectedTotalSpecs;
            }
            item.Product = new ShoppingCartProduct(product, selectedTotalSpecs);
            item.Quantity = quantity;
            item.ShoppingCartItemId = Guid.NewGuid().ToString();
            shoppingCart.Add(item);
            SaveShoppingCart(shoppingCart);
            PaymentLogger.Info(JsonConvert.SerializeObject(shoppingCart));
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [NoCache]
        public ActionResult GetShoppingCartSmallDetails()
        {
            var shoppingCart = GetShoppingCartFromDataSource();
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\ShoppingCartTemplates\_ShoppingCartSmallDetails.cshtml",
                        new ViewDataDictionary(shoppingCart), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [NoCache]
        public ActionResult GetShoppingCartLinks()
        {
            var shoppingCart = GetShoppingCartFromDataSource();
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
            var shoppingCart = GetShoppingCartFromDataSource();
            return PartialView("ShoppingCartTemplates/_ShoppingCartLinks", shoppingCart);
        }

        private void SaveShoppingCart(ShoppingCartSession shoppingCart)
        {
            var item = new ShoppingCart();
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.Name = shoppingCart.OrderGuid;
            item.IsActive = false;
            item.Lang = CurrentLanguage;
            item.Position = 0;
            item.ShoppingCartJson = JsonConvert.SerializeObject(shoppingCart);
            item.OrderGuid = shoppingCart.OrderGuid;
            string userId = shoppingCart.Customer != null ? shoppingCart.Customer.UserId : "";
            item.UserId = string.IsNullOrEmpty(userId) ? getUserId() : userId;
            ShoppingCartService.SaveOrEditShoppingCart(item);
        }

        private string getUserId()
        {
            if (Request.IsAuthenticated)
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (user != null)
                {
                   return user.Id;
                }
            }
            return string.Empty;
        }

        private ShoppingCartSession GetShoppingCartFromDataSource()
        {
            HttpCookie orderGuid = Request.Cookies[Domain.Constants.OrderGuidCookieKey];
            string orderGuid2 = orderGuid == null ? null : orderGuid.Value;
            return GetShoppingCartByOrderGuid(orderGuid2);
        }

        private ShoppingCartSession GetShoppingCartByOrderGuid(string orderGuid)
        {
            ShoppingCartSession result = null;
            var item = orderGuid != null ? ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid) : null;
            if (item == null)
            {
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
                GetCustomerIfAuthenticated(result);
            }
            else
            {
                result = JsonConvert.DeserializeObject<ShoppingCartSession>(item.ShoppingCartJson);
                string userId = result.Customer != null ? result.Customer.UserId : "";
                item.UserId = string.IsNullOrEmpty(userId) ? getUserId() : userId;
            }
        
            result.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
            result.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
            result.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);
            return result;
        }

        private void GetCustomerIfAuthenticated(ShoppingCartSession result)
        {
            if (Request.IsAuthenticated)
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (user != null)
                {
                    var c = CustomerService.GetUserId(user.Id);
                    if(c == null)
                    {
                        c = new Customer();
                        c.UserId = user.Id;
                    }
                    result.Customer = c;
                    c.IsSameAsShippingAddress = true;
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
                if (shoppingCart.Customer == null)
                {
                    shoppingCart.Customer = new Customer();
                }
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
                var user = UserManager.FindByName(User.Identity.GetUserName());
                shoppingCart.Customer.UserId = user.Id;
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
            address.Description = customer.RegistrationAddress;
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

        public ActionResult renderShoppingCartPrice()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return Json(new
            {
                status = Domain.Constants.SUCCESS,
                CargoPriceInt = shoppingCart.CargoPriceValue,
                CargoPrice = shoppingCart.CargoPriceValue.CurrencySign(),
                BasketMinTotalPriceForCargoInt = shoppingCart.BasketMinTotalPriceForCargoInt,
                BasketMinTotalPriceForCargo = shoppingCart.BasketMinTotalPriceForCargoInt.CurrencySign(),
                TotalPriceWithCargoPriceDouble = shoppingCart.TotalPriceWithCargoPrice,
                TotalPriceWithCargoPrice = shoppingCart.TotalPriceWithCargoPrice.CurrencySign(),
                TotalPriceDouble = shoppingCart.TotalPrice,
                TotalPrice = shoppingCart.TotalPrice.CurrencySign()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult sendOrderComments(string orderComments, string orderGuid)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            shoppingCart.OrderComments = orderComments;
            SaveShoppingCart(shoppingCart);
            return Json(new { status = Domain.Constants.SUCCESS }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateQuantity(String shoppingItemId, int quantity)
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                item.Quantity = quantity;
                SaveShoppingCart(shoppingCart);
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId }, JsonRequestBehavior.AllowGet);
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
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PlaceOrder()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
       
            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsEmpty())
            {
                return Content("ShoppingCartItems is EMPTY");
            }
            if (shoppingCart.Customer.isValid() && shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                ViewBag.CheckoutFormInitialize = iyzicoService.CreateCheckoutFormInitialize(shoppingCart, user.Id);
                return View(shoppingCart);
            }
            else
            {
                return Content("RegisterCustomer");
            }
        }

        public ActionResult PaymentResult(RetrieveCheckoutFormRequest model, string o, string u)
        {
            CheckoutForm checkoutForm = iyzicoService.GetCheckoutForm(model);
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                ShoppingCartSession shoppingCart = GetShoppingCartByOrderGuid(orderGuid);
                var userId = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(u));
                var order = ShoppingCartService.SaveShoppingCart(shoppingCart, checkoutForm, userId);
                SendEmails(order);
                ClearCart(shoppingCart);
                return RedirectToAction("ThankYouForYourOrder", new { orderId = order.Id });
            }
            else
            {
                PaymentLogger.Error("CheckoutForm NOT SUCCESS:" + JsonConvert.SerializeObject(checkoutForm));
                return RedirectToAction("NoSuccessForYourOrder");
            }
        }
        private void SendCompanyGotNewOrderEmail(Order order)
        {
            var emailTemplate = RazorEngineHelper.CompanyGotNewOrderEmail(order.Id);
            EmailSender.SendRenderedEmailTemplate(SettingService.GetEmailAccount(), emailTemplate);
        }
        private void SendEmails(Order order)
        {
            var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(order.Id);
            EmailSender.SendRenderedEmailTemplate(SettingService.GetEmailAccount(), emailTemplate);
        }

        public ActionResult ThankYouForYourOrder(int orderId)
        {
            return View(OrderService.GetSingle(orderId));
        }
        public ActionResult NoSuccessForYourOrder()
        {
            return View();
        }


        private void ClearCart(ShoppingCartSession shoppingCart)
        {
            if (Request.Browser.Cookies)
            {
                Response.Cookies.Remove(Domain.Constants.OrderGuidCookieKey);
                var aCookie = new HttpCookie(Domain.Constants.OrderGuidCookieKey) { Expires = DateTime.Now.AddDays(-1) };
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