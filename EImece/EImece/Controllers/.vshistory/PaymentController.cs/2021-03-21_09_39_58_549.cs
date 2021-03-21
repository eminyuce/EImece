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
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
   // [RoutePrefix(EImece.Domain.Constants.PaymentControllerRoutingPrefix)]
    public class PaymentController : BaseController
    {
        private static readonly Logger PaymentLogger = LogManager.GetCurrentClassLogger();
       

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public IAddressService AddressService { get; set; }
        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IyzicoService IyzicoService { get; set; }

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        public PaymentController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
       // [Route(Domain.Constants.ShoppingCartPrefix)]
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
        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult HomePageShoppingCart()
        {
            return PartialView("ShoppingCartTemplates/_HomePageShoppingCart", GetShoppingCart());
        }

        public ActionResult AddToCart(string productId, int quantity, string orderGuid, string productSpecItems)
        {
            int pId = GeneralHelper.RevertId(productId);
            var product = ProductService.GetProductById(pId);
            if(product != null)
            {
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
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
          
          
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
                    if (c == null)
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

        

        public ActionResult CheckoutBillingDetails()
        {
            if (Request.IsAuthenticated)
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                if (shoppingCart.ShoppingCartItems.IsNotEmpty())
                {
                    if (shoppingCart.Customer == null)
                    {
                        shoppingCart.Customer = new Customer();
                        shoppingCart.Customer.Country = "Türkiye";
                    }
                    if (shoppingCart.Customer.IsEmpty())
                    {
                        GetCustomerIfAuthenticated(shoppingCart);
                    }
                    return View(shoppingCart);
                }
                else
                {
                    return RedirectToAction("shoppingcart", "Payment");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("CheckoutPaymentOrderReview", "Payment") });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckoutBillingDetails(Customer customer)
        {
            bool isValidCustomer = customer != null && customer.isValidCustomer();
            if (isValidCustomer)
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
                InformCustomerToFillOutForm(customer);
                ShoppingCartSession shoppingCart = GetShoppingCart();
                shoppingCart.Customer = customer;
                return View(shoppingCart);
            }
        }

        public ActionResult CheckoutDelivery()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }

        public ActionResult CheckoutPaymentOrderReview()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            if (shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                return View(shoppingCart);
            }
            else
            {
                return RedirectToAction("shoppingcart", "Payment");
            }
        }
        // Sepetteki guncelle butona basilinca calisan ActionResult
        public ActionResult renderShoppingCartPrice()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            String cargoPriceHtml = "";
            if(shoppingCart.CargoPriceValue == 0)
            {
                cargoPriceHtml = string.Format("<span class='badge badge-pill badge-danger mr-2 mb-2'>{0}</span>", Resource.CargoFreeTextInfo);
            }
            else
            {
                cargoPriceHtml = string.Format("<span>{0}:</span><span>{1}</span>", AdminResource.CargoPrice, shoppingCart.CargoPriceValue.CurrencySign());
            }
            return Json(new
            {
                status = Domain.Constants.SUCCESS,
                CargoPriceHtml= cargoPriceHtml,
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
                return RedirectToAction("shoppingcart", "Payment");
            }
            if (shoppingCart.Customer.isValidCustomer() && shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitialize(shoppingCart, user.Id);
                return View(shoppingCart);
            }
            else
            {
                return Content("RegisterCustomer");
            }
        }

        public ActionResult PaymentResult(RetrieveCheckoutFormRequest model, string o, string u)
        {
            CheckoutForm checkoutForm = IyzicoService.GetCheckoutForm(model);
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
        public ActionResult BuyNow(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var productId = id.GetId();
                BuyNowModel buyNowModel = CreateBuyNowModel(productId);
                ViewBag.SeoId = buyNowModel.ProductDetailViewModel.Product.GetSeoUrl();
                return View(buyNowModel);
            }
            catch (Exception e)
            {
                PaymentLogger.Error(e, "Products.BuyNow page");
                return RedirectToAction("InternalServerError", "Error");
            }
        }
        private BuyNowModel CreateBuyNowModel(int productId)
        {
            BuyNowModel buyNowModel = new BuyNowModel();
            buyNowModel.ProductId = productId;
            buyNowModel.ProductDetailViewModel = ProductService.GetProductDetailViewModelById(productId);
            buyNowModel.ShoppingCartItem = new ShoppingCartItem();
            buyNowModel.ShoppingCartItem.Product = new ShoppingCartProduct(ProductService.GetProductById(productId), new List<ProductSpecItem>());
            buyNowModel.ShoppingCartItem.Quantity = 1;
            buyNowModel.ShoppingCartItem.ShoppingCartItemId = Guid.NewGuid().ToString();
            buyNowModel.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
            buyNowModel.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
            buyNowModel.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);
            return buyNowModel;
        }

        [HttpPost]
        public ActionResult BuyNow(String productId, Customer customer)
        {
            bool isValidCustomer = customer != null && customer.isValidCustomer();
            BuyNowModel buyNowModel = CreateBuyNowModel(GeneralHelper.RevertId(productId));
            buyNowModel.Customer = customer;

            if (isValidCustomer)
            {
                buyNowModel.ShippingAddress = SetAddress(customer, buyNowModel.ShippingAddress);
                buyNowModel.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                buyNowModel.OrderGuid = Guid.NewGuid().ToString();

                var item = new ShoppingCart();
                item.CreatedDate = DateTime.Now;
                item.UpdatedDate = DateTime.Now;
                item.Name = buyNowModel.OrderGuid;
                item.IsActive = false;
                item.Lang = CurrentLanguage;
                item.Position = 0;
                item.ShoppingCartJson = JsonConvert.SerializeObject(buyNowModel);
                item.OrderGuid = buyNowModel.OrderGuid;
                item.UserId = Domain.Constants.BuyNowCustomerUserId;
                ShoppingCartService.SaveOrEditShoppingCart(item);
      
                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitializeBuyNow(buyNowModel);
               


                return View("BuyNowPayment", buyNowModel);
            }
            else
            {
                InformCustomerToFillOutForm(customer);
                return View(buyNowModel);
            }

        }

    

        public ActionResult BuyNowPaymentResult(RetrieveCheckoutFormRequest model, String o)
        {
            var checkoutForm = IyzicoService.GetCheckoutForm(model);
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                var item = ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid);
                BuyNowModel buyNowModel = JsonConvert.DeserializeObject<BuyNowModel>(item.ShoppingCartJson);
                buyNowModel.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
                buyNowModel.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
                buyNowModel.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);

                var order = ShoppingCartService.SaveBuyNow(buyNowModel, checkoutForm);
               // SendEmails(order);


                return RedirectToAction("ThankYouForYourOrder", new { orderId = order.Id });
            }
            else
            {
                PaymentLogger.Error("CheckoutForm NOT SUCCESS:" + JsonConvert.SerializeObject(checkoutForm));
                return RedirectToAction("NoSuccessForYourOrder");
            }
        }
        protected void InformCustomerToFillOutForm(Customer customer)
        {
            if (customer == null)
            {
                throw new NotSupportedException();
            }
            if (string.IsNullOrEmpty(customer.Name.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Name", Resource.PleaseEnterYourName);
            }
            if (string.IsNullOrEmpty(customer.Surname.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Surname", Resource.PleaseEnterYourSurname);
            }
            if (string.IsNullOrEmpty(customer.GsmNumber.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.GsmNumber", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Email.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Email", Resource.PleaseEnterYourEmail);
            }
            if (string.IsNullOrEmpty(customer.City.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.City", Resource.PleaseEnterYourCity);
            }
            if (string.IsNullOrEmpty(customer.Town.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Town", Resource.PleaseEnterYourTown);
            }
            if (string.IsNullOrEmpty(customer.Country.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Country", Resource.PleaseEnterYourCountry);
            }
            if (string.IsNullOrEmpty(customer.District.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.District", Resource.PleaseEnterYourDistrict);
            }
            if (string.IsNullOrEmpty(customer.Street.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Street", Resource.PleaseEnterYourStreet);
            }
            if (string.IsNullOrEmpty(customer.IdentityNumber.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.IdentityNumber", Resource.MandatoryField);
            }
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
        }

        protected Domain.Entities.Address SetAddress(Customer customer, Domain.Entities.Address address)
        {
            if (address == null)
            {
                address = new Domain.Entities.Address();
            }
            if (customer == null)
            {
                throw new NotSupportedException();
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

        protected void SendEmails(Order order)
        {
            try
            {
                var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(order.Id);
                EmailSender.SendRenderedEmailTemplateToCustomer(SettingService.GetEmailAccount(), emailTemplate);
            }
            catch (Exception e)
            {
                PaymentLogger.Error(e, "OrderConfirmationEmail exception");
            }

            try
            {
                var emailTemplate = RazorEngineHelper.CompanyGotNewOrderEmail(order.Id);
                EmailSender.SendRenderedEmailTemplateToAdminUsers(SettingService.GetEmailAccount(), emailTemplate);
            }
            catch (Exception e)
            {
                PaymentLogger.Error(e, "CompanyGotNewOrderEmail exception");
            }
        }
    }
}