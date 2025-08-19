using EImece.Domain;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class PaymentController : BaseController
    {
        
        private static readonly Logger PaymentLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        [Inject]
        public ICouponService CouponService { get; set; }

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

        public ActionResult ShoppingCart()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                PaymentLogger.Info($"Setting UrlReferrer to: {urlReferrer}");
                shoppingCart.UrlReferrer = urlReferrer.ToStr();
            }
            PaymentLogger.Info("Returning ShoppingCart view.");
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
            if (quantity < 0 || quantity > 1000)
            {
                PaymentLogger.Error("Quantity cannot be less than 0 or greater than 1000.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(productId))
            {
                PaymentLogger.Error("Product ID cannot be null or empty.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(orderGuid))
            {
                PaymentLogger.Error("OrderGuid cannot be null or empty.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
            PaymentLogger.Info($"Entering AddToCart action with productId: {productId}, quantity: {quantity}, orderGuid: {orderGuid}");
            int pId = GeneralHelper.RevertId(productId);
            PaymentLogger.Info($"Reverted productId to: {pId}");
            var product = ProductService.GetProductById(pId);
            if (product != null)
            {
                PaymentLogger.Info($"Product found with ID: {pId}");
                var shoppingCart = GetShoppingCart();
                if (string.IsNullOrEmpty(shoppingCart.OrderGuid))
                {
                    shoppingCart.OrderGuid = orderGuid;
                }
                else if (!shoppingCart.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"OrderGuid does not match. Setting new OrderGuid: {orderGuid}");
                }
               
                PaymentLogger.Info($"Set shopping cart OrderGuid to: {orderGuid}");

                var item = new ShoppingCartItem();
                var selectedTotalSpecs = new List<ProductSpecItem>();
                if (!string.IsNullOrEmpty(productSpecItems))
                {
                    PaymentLogger.Info("Deserializing productSpecItems.");
                    var ooo = JsonConvert.DeserializeObject<ProductSpecItemRoot>(productSpecItems);
                    selectedTotalSpecs = ooo.selectedTotalSpecs;
                    PaymentLogger.Info($"Found {selectedTotalSpecs.Count} product specifications.");
                }
                item.Product = new ShoppingCartProduct(product, selectedTotalSpecs);
                item.Quantity = quantity;
                item.ShoppingCartItemId = Guid.NewGuid().ToString();
                PaymentLogger.Info($"Created shopping cart item with ID: {item.ShoppingCartItemId}");
                shoppingCart.Add(item);
                PaymentLogger.Info("Added item to shopping cart.");
                SaveShoppingCart(shoppingCart);
                PaymentLogger.Info("Returning success JSON response.");
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                PaymentLogger.Error($"Product not found with ID: {pId}");
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
            PaymentLogger.Info("Returning JSON response with HTML.");
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
            PaymentLogger.Info("Returning JSON response with HTML.");
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        [NoCache]
        public ActionResult ShoppingCartLink()
        {
            var shoppingCart = GetShoppingCartFromDataSource();
            PaymentLogger.Info("Rendering _ShoppingCartLinks partial view.");
            return PartialView("ShoppingCartTemplates/_ShoppingCartLinks", shoppingCart);
        }

        private ShoppingCart SaveShoppingCart(ShoppingCartSession shoppingCart)
        {
            PaymentLogger.Info("Entering SaveShoppingCart method.");
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
            PaymentLogger.Info($"Saving shopping cart with OrderGuid: {item.OrderGuid}, UserId: {item.UserId}");

            shoppingCart.CurrentLanguage = CurrentLanguage;
            ShoppingCartService.SaveOrEditShoppingCart(item);
            PaymentLogger.Info("Shopping cart saved to data source.");

            return item;
        }

        private string getUserId()
        {
            if (Request.IsAuthenticated)
            {
                PaymentLogger.Info("Request is authenticated.");
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (user != null)
                {
                    PaymentLogger.Info($"User found with ID: {user.Id}");
                    return user.Id;
                }
                PaymentLogger.Info("No user found.");
            }
            return string.Empty;
        }

        private ShoppingCartSession GetShoppingCartFromDataSource()
        {
            PaymentLogger.Info("Entering GetShoppingCartFromDataSource method.");
            HttpCookie orderGuid = Request.Cookies[Domain.Constants.OrderGuidCookieKey];
            string orderGuid2 = orderGuid == null ? null : orderGuid.Value;
            PaymentLogger.Info($"Retrieved OrderGuid from cookie: {orderGuid2}");
            var result = GetShoppingCartByOrderGuid(orderGuid2);
            PaymentLogger.Info("Shopping cart retrieved from GetShoppingCartByOrderGuid.");
            return result;
        }

        private ShoppingCartSession GetShoppingCartByOrderGuid(string orderGuid)
        {
            ShoppingCartSession result = null;
            var item = orderGuid != null ? ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid) : null;
            if (item == null)
            {
                PaymentLogger.Info("No existing shopping cart found. Creating default shopping cart.");
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
                GetCustomerIfAuthenticated(result);
            }
            else
            {
                PaymentLogger.Info("Existing shopping cart found. Deserializing JSON.");
                result = JsonConvert.DeserializeObject<ShoppingCartSession>(item.ShoppingCartJson);
                string userId = result.Customer != null ? result.Customer.UserId : "";
                item.UserId = string.IsNullOrEmpty(userId) ? getUserId() : userId;
                PaymentLogger.Info($"Updated shopping cart UserId to: {item.UserId}");
            }

            result.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
            result.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
            result.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);
            PaymentLogger.Info("Set cargo details for shopping cart.");
            return result;
        }

        private void GetCustomerIfAuthenticated(ShoppingCartSession result)
        {
            if (Request.IsAuthenticated)
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (user != null)
                {
                    PaymentLogger.Info($"User found with ID: {user.Id}");
                    var c = CustomerService.GetUserId(user.Id);
                    if (c == null)
                    {
                        PaymentLogger.Info("No customer found. Creating new customer.");
                        c = new Customer();
                        c.UserId = user.Id;
                        c.CustomerType = (int)EImeceCustomerType.Normal;
                    }
                    result.Customer = c;
                    c.IsSameAsShippingAddress = true;
                }
                else
                {
                    throw new ArgumentException("User cannot be null"+User.Identity.GetUserName());
                }
            }
            else
            {
                PaymentLogger.Info("Request is not authenticated. No customer assigned.");
            }
        }

        private ShoppingCartSession GetShoppingCart()
        {
            PaymentLogger.Info("Entering GetShoppingCart method.");
            var shoppingCart = GetShoppingCartFromDataSource();
            PaymentLogger.Info("Shopping cart retrieved.");
            return shoppingCart;
        }

        public ActionResult CheckoutBillingDetails()
        {
            if (Request.IsAuthenticated)
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                if (shoppingCart.ShoppingCartItems.IsNotEmpty())
                {
                    PaymentLogger.Info("Shopping cart has items.");
                    if (shoppingCart.Customer == null)
                    {
                        PaymentLogger.Info("No customer in shopping cart. Creating new customer.");
                        shoppingCart.Customer = new Customer();
                        shoppingCart.Customer.CustomerType = (int)EImeceCustomerType.Normal;
                        shoppingCart.Customer.Country = Domain.Constants.IYZICO_ADDRESS_COUNTRY;
                        shoppingCart.Customer.Ip = GeneralHelper.GetIpAddress();
                    }
                    if (shoppingCart.Customer.IsEmpty())
                    {
                        PaymentLogger.Info("Customer is empty. Populating from authenticated user.");
                        GetCustomerIfAuthenticated(shoppingCart);
                    }
                    PaymentLogger.Info("Returning CheckoutBillingDetails view.");
                    return View(shoppingCart);
                }
                else
                {
                    PaymentLogger.Info("Shopping cart is empty. Redirecting to shoppingcart.");
                    return RedirectToAction("shoppingcart", "Payment");
                }
            }
            else
            {
                PaymentLogger.Info("User is not authenticated. Redirecting to login.");
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("CheckoutPaymentOrderReview", "Payment") });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckoutBillingDetails(Customer customer)
        {
            PaymentLogger.Info("Entering CheckoutBillingDetails POST action.");
            if (customer == null)
            {
                PaymentLogger.Error("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }
            bool isValidCustomer = customer.isValidCustomer();
            PaymentLogger.Info($"Customer validation result: {isValidCustomer}");
            if (isValidCustomer)
            {
                ShoppingCartSession shoppingCart = GetShoppingCart();
                customer.CustomerType = (int)EImeceCustomerType.Normal;
                shoppingCart.Customer = customer;
                var user = UserManager.FindByName(User.Identity.GetUserName());
                shoppingCart.Customer.UserId = user.Id;
                PaymentLogger.Info($"Assigned UserId: {user.Id} to customer.");
                if (customer.IsSameAsShippingAddress)
                {
                    PaymentLogger.Info("Shipping address is same as billing address.");
                }

                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                PaymentLogger.Info("Set shipping and billing addresses.");

                SaveShoppingCart(shoppingCart);
                PaymentLogger.Info("Shopping cart saved with billing details.");
                PaymentLogger.Info("Redirecting to CheckoutPaymentOrderReview.");
                return RedirectToAction("CheckoutPaymentOrderReview");
            }
            else
            {
                PaymentLogger.Info("Customer validation failed. Informing customer.");
                InformCustomerToFillOutForm(customer);
                ShoppingCartSession shoppingCart = GetShoppingCart();
                shoppingCart.Customer = customer;
                return View(shoppingCart);
            }
        }

        public ActionResult CargoTracking(string id)
        {
            ViewBag.OrderNumber = id;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CargoTrackingResult(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var order = OrderService.GetByOrderNumber(orderNumber);
            if (order == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\CargoTrackingResult.cshtml",
                        new ViewDataDictionary(order), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
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
                PaymentLogger.Info("Shopping cart has items. Returning view.");
                return View(shoppingCart);
            }
            else
            {
                PaymentLogger.Info("Shopping cart is empty. Redirecting to shoppingcart.");
                return RedirectToAction("shoppingcart", "Payment");
            }
        }

        public ActionResult renderShoppingCartPrice()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            String cargoPriceHtml = "";
            if (shoppingCart.CargoPriceValue == 0)
            {
                PaymentLogger.Info("Cargo price is 0. Setting free shipping HTML.");
                cargoPriceHtml = string.Format("<span class='badge badge-pill badge-danger mr-2 mb-2'>{0}</span>", Resource.CargoFreeTextInfo);
            }
            else
            {
                PaymentLogger.Info($"Cargo price is {shoppingCart.CargoPriceValue}. Formatting HTML.");
                cargoPriceHtml = string.Format("<span>{0}:</span><span>{1}</span>", AdminResource.CargoPrice, shoppingCart.CargoPriceValue.CurrencySign());
            }
            return Json(new
            {
                status = Domain.Constants.SUCCESS,
                CargoPriceHtml = cargoPriceHtml,
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
            PaymentLogger.Info($"Entering sendOrderComments with orderComments: {orderComments}, orderGuid: {orderGuid}");
            ShoppingCartSession shoppingCart = GetShoppingCart();
            shoppingCart.OrderComments = orderComments;
            PaymentLogger.Info("Order comments assigned to shopping cart.");
            SaveShoppingCart(shoppingCart);
            PaymentLogger.Info("Returning success JSON response.");
            return Json(new { status = Domain.Constants.SUCCESS }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateQuantity(String shoppingItemId, int quantity)
        {
            PaymentLogger.Info($"Entering UpdateQuantity with shoppingItemId: {shoppingItemId}, quantity: {quantity}");
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                PaymentLogger.Info($"Found item with ID: {shoppingItemId}. Updating quantity to: {quantity}");
                item.Quantity = quantity;
                SaveShoppingCart(shoppingCart);
                PaymentLogger.Info("Shopping cart saved with updated quantity.");
                PaymentLogger.Info("Returning success JSON response.");
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                PaymentLogger.Error($"Item with ID: {shoppingItemId} not found.");
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveCart(String shoppingItemId)
        {
            PaymentLogger.Info($"Entering RemoveCart with shoppingItemId: {shoppingItemId}");
            ShoppingCartSession shoppingCart = GetShoppingCart();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                PaymentLogger.Info($"Found item with ID: {shoppingItemId}. Removing from cart.");
                shoppingCart.ShoppingCartItems.Remove(item);
                if (shoppingCart.ShoppingCartItems.IsEmpty())
                {
                    PaymentLogger.Info("Shopping cart is now empty. Clearing coupon.");
                    shoppingCart.Coupon = null;
                }
                SaveShoppingCart(shoppingCart);
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                PaymentLogger.Error($"Item with ID: {shoppingItemId} not found.");
                PaymentLogger.Info("Returning failed JSON response.");
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PlaceOrder()
        {
            PaymentLogger.Info("Entering PlaceOrder action.");
            ShoppingCartSession shoppingCart = GetShoppingCart();

            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsEmpty())
            {
                PaymentLogger.Info("Shopping cart is null or empty. Redirecting to shoppingcart.");
                return RedirectToAction("shoppingcart", "Payment");
            }
            if (shoppingCart.Customer.isValidCustomer() && shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                PaymentLogger.Info("Customer is valid and cart has items.");
                if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account"); // or the appropriate controller/action
                }
                else
                {
                    var user = UserManager.FindByName(User.Identity.GetUserName());
                    PaymentLogger.Info($"Initializing checkout form for user ID: {user.Id}");
                    ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitialize(shoppingCart, user.Id);
                    PaymentLogger.Info("Returning PlaceOrder view.");
                    return View(shoppingCart);
                }
            }
            else
            {
                return Content("RegisterCustomer");
            }
        }
        
        public ActionResult PaymentResult(RetrieveCheckoutFormRequest model, string o, string u, String orderNumber)
        {
            CheckoutForm checkoutForm = IyzicoService.GetCheckoutForm(model);
            PaymentLogger.Info($"PaymentResult with ACCOUNT status: {checkoutForm.PaymentStatus} ConversationId: {checkoutForm.ConversationId}");
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                PaymentLogger.Info($"Decrypted orderGuid: {orderGuid}");
                ShoppingCartSession shoppingCart = GetShoppingCartByOrderGuid(orderGuid);
                var userId = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(u));
                PaymentLogger.Info($"Decrypted userId: {userId}");
                var order = ShoppingCartService.SaveShoppingCart(orderNumber, shoppingCart, checkoutForm, userId);
                PaymentLogger.Info($"Order saved with ID: {order.Id}");
                SendNotificationEmailsToCustomerAndAdminUsersForNewOrder(OrderService.GetOrderById(order.Id));
                ClearCart(shoppingCart);
                PaymentLogger.Info("Cart cleared. Redirecting to ThankYouForYourOrder.");
                return RedirectToAction("ThankYouForYourOrder", new { orderId = order.Id });
            }
            else
            {
                PaymentLogger.Error($"Payment failed. CheckoutForm: {JsonConvert.SerializeObject(checkoutForm)}");
                return RedirectToAction("NoSuccessForYourOrder");
            }
        }

        public ActionResult ThankYouForYourOrder(int orderId)
        {
            var order = OrderService.GetOrderById(orderId);
            //SendNotificationEmailsToCustomerAndAdminUsersForNewOrder(OrderService.GetOrderById(order.Id));
            return View(order);
        }

        public ActionResult NoSuccessForYourOrder()
        {
            return View();
        }

        private void ClearCart(ShoppingCartSession shoppingCart)
        {
            PaymentLogger.Info("Entering ClearCart method.");
            if (Request.Browser.Cookies)
            {
                PaymentLogger.Info("Removing OrderGuid cookie.");
                Response.Cookies.Remove(Domain.Constants.OrderGuidCookieKey);
                var aCookie = new HttpCookie(Domain.Constants.OrderGuidCookieKey) { 
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure  = Request.IsSecureConnection
                };
                Response.Cookies.Add(aCookie);
                PaymentLogger.Info("Added expired cookie to response.");
            }
            ShoppingCartService.DeleteByOrderGuid(shoppingCart.OrderGuid);
            PaymentLogger.Info($"Deleted shopping cart with OrderGuid: {shoppingCart.OrderGuid}");
        }

        public ActionResult PaymentSuccess(RetrieveCheckoutFormRequest model)
        {
            return Content("PaymentSuccess is done");
        }

        public ActionResult BuyNow(String id)
        {
            PaymentLogger.Info($"Entering BuyNow with id: {id}");
            if (String.IsNullOrEmpty(id))
            {
                PaymentLogger.Error("Product ID is null or empty.");
                PaymentLogger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var productId = id.GetId();
                PaymentLogger.Info($"Converted product ID to: {productId}");
                BuyNowModel buyNowModel = CreateBuyNowModel(productId);
                PaymentLogger.Info("Created BuyNow model.");
                ViewBag.SeoId = buyNowModel.ProductDetailViewModel.Product.GetSeoUrl();
                PaymentLogger.Info($"Set SeoId in ViewBag: {ViewBag.SeoId}");
                PaymentLogger.Info("Returning BuyNow view.");
                return View(buyNowModel);
            }
            catch (Exception e)
            {
                PaymentLogger.Error($"Exception in BuyNow: {e.Message}", e);
                PaymentLogger.Info("Redirecting to InternalServerError.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        private BuyNowModel CreateBuyNowModel(int productId)
        {
            PaymentLogger.Info($"Entering CreateBuyNowModel with productId: {productId}");
            BuyNowModel buyNowModel = new BuyNowModel();
            buyNowModel.ProductId = productId;
            buyNowModel.ProductDetailViewModel = ProductService.GetProductDetailViewModelById(productId);
            PaymentLogger.Info("Set product details in BuyNow model.");
            buyNowModel.ShoppingCartItem = new ShoppingCartItem();
            buyNowModel.ShoppingCartItem.Product = new ShoppingCartProduct(ProductService.GetProductById(productId), new List<ProductSpecItem>());
            buyNowModel.ShoppingCartItem.Quantity = 1;
            buyNowModel.ShoppingCartItem.ShoppingCartItemId = Guid.NewGuid().ToString();
            PaymentLogger.Info($"Created shopping cart item with ID: {buyNowModel.ShoppingCartItem.ShoppingCartItemId}");
            buyNowModel.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
            buyNowModel.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
            buyNowModel.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);
            PaymentLogger.Info("Set cargo details in BuyNow model.");
            return buyNowModel;
        }

        [HttpPost]
        public ActionResult BuyNow(String productId, Customer customer)
        {
            PaymentLogger.Info($"Entering BuyNow POST with productId: {productId}");
            if (customer == null)
            {
                PaymentLogger.Error("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }

            bool isValidCustomer = customer.isValidCustomer();
            PaymentLogger.Info($"Customer validation result: {isValidCustomer}");
            BuyNowModel buyNowModel = CreateBuyNowModel(GeneralHelper.RevertId(productId));
            buyNowModel.Customer = customer;
            PaymentLogger.Info("Assigned customer to BuyNow model.");

            if (isValidCustomer)
            {
                customer.CustomerType = (int)EImeceCustomerType.BuyNow;
                buyNowModel.ShippingAddress = SetAddress(customer, buyNowModel.ShippingAddress);
                buyNowModel.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                buyNowModel.OrderGuid = Guid.NewGuid().ToString();
                PaymentLogger.Info($"Set shipping address and OrderGuid: {buyNowModel.OrderGuid}");

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
                PaymentLogger.Info("Saved BuyNow shopping cart.");

                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitializeBuyNow(buyNowModel);
                PaymentLogger.Info("Initialized checkout form for BuyNow.");
                PaymentLogger.Info("Returning BuyNowPayment view.");
                return View("BuyNowPayment", buyNowModel);
            }
            else
            {
                PaymentLogger.Info("Customer validation failed. Informing customer.");
                InformCustomerToFillOutForm(customer);
                PaymentLogger.Info("Returning BuyNow view with validation errors.");
                return View(buyNowModel);
            }
        }

        public ActionResult BuyNowPaymentResult(RetrieveCheckoutFormRequest model, String o)
        {
            PaymentLogger.Info("Entering BuyNowPaymentResult action.");
            CheckoutForm checkoutForm = IyzicoService.GetCheckoutForm(model);
            PaymentLogger.Info($"Payment status: {checkoutForm.PaymentStatus}");
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                PaymentLogger.Info($"Decrypted orderGuid: {orderGuid}");
                var item = ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid);
                BuyNowModel buyNowModel = JsonConvert.DeserializeObject<BuyNowModel>(item.ShoppingCartJson);
                PaymentLogger.Info("Deserialized BuyNow model from shopping cart.");
                if (buyNowModel.ShoppingCartItem == null || buyNowModel.ShoppingCartItem.Product == null)
                {
                    PaymentLogger.Error("ShoppingCartItem or Product is null in BuyNow model.");
                    throw new ArgumentException("buyNowModel.ShoppingCartItem.Product cannot be null");
                }
                if (buyNowModel.Customer == null)
                {
                    PaymentLogger.Error("Customer is null in BuyNow model.");
                    throw new ArgumentException("buyNowModel.Customer cannot be null");
                }
                buyNowModel.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
                buyNowModel.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
                buyNowModel.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);
                buyNowModel.Customer.Lang = CurrentLanguage;
                PaymentLogger.Info("Updated BuyNow model with cargo and language details.");

                var order = ShoppingCartService.SaveBuyNow(buyNowModel, checkoutForm);
                PaymentLogger.Info($"Order saved with ID: {order.Id}");
                ClearBuyNow(buyNowModel);
                PaymentLogger.Info("Cleared BuyNow cart. Redirecting to ThankYouForYourOrder.");
                return RedirectToAction("ThankYouForYourOrder", new { orderId = order.Id });
            }
            else
            {
                PaymentLogger.Error($"BuyNow payment failed. CheckoutForm: {JsonConvert.SerializeObject(checkoutForm)}");
                PaymentLogger.Info("Redirecting to NoSuccessForYourOrder.");
                return RedirectToAction("NoSuccessForYourOrder");
            }
        }

        [HttpPost]
        public ActionResult ApplyCoupon(String couponCode)
        {
            var couponObj = CouponService.GetCouponByCode(couponCode, CurrentLanguage);
            var shoppingCart = GetShoppingCartFromDataSource();
            if (couponObj != null)
            {
                shoppingCart.Coupon = couponObj;
            }
            else
            {
                shoppingCart.Coupon = null;
            }
            SaveShoppingCart(shoppingCart);
            return RedirectToAction("shoppingcart");
        }

        private void ClearBuyNow(BuyNowModel buyNowModel)
        {
            PaymentLogger.Info($"Entering ClearBuyNow with OrderGuid: {buyNowModel.OrderGuid}");
            ShoppingCartService.DeleteByOrderGuid(buyNowModel.OrderGuid);
            PaymentLogger.Info("BuyNow cart deleted from data source.");
        }

        protected void InformCustomerToFillOutForm(Customer customer)
        {
            PaymentLogger.Info("Entering InformCustomerToFillOutForm method.");
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
            else
            {
                if (GeneralHelper.IsGsmNumberNotValid(customer.GsmNumber.ToStr()))
                {
                    ModelState.AddModelError("customer.GsmNumber", Resource.GsmNumberNotValidMessage);
                }
            }
            if (string.IsNullOrEmpty(customer.Email.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Email", Resource.PleaseEnterYourEmail);
            }
            else
            {
                if (GeneralHelper.IsNotValidEmail(customer.Email.ToStr()))
                {
                    ModelState.AddModelError("customer.Email", Resource.EmailNotValidMessage);
                }
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
             
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
        }

        protected Domain.Entities.Address SetAddress(Customer customer, Domain.Entities.Address address)
        {
            PaymentLogger.Info("Entering SetAddress method.");
            if (address == null)
            {
                PaymentLogger.Info("Address is null. Creating new address.");
                address = new Domain.Entities.Address();
            }
            if (customer == null)
            {
                PaymentLogger.Error("Customer is null. Throwing exception.");
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
            PaymentLogger.Info("Address set with customer details.");
            return address;
        }

        protected void SendNotificationEmailsToCustomerAndAdminUsersForNewOrder(Order order)
        {
            PaymentLogger.Info($"Entering SendEmails for order ID: {order.Id}");
            try
            {
                var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(order.Id);
                if (emailTemplate.Item2.Result == null)
                {
                    PaymentLogger.Error("RazorEngineHelper OrderConfirmationEmail template Is NULL.order.Id:" + order.Id);
                }
                PaymentLogger.Info("Generated order confirmation email template.");
                EmailSender.SendRenderedEmailTemplateToCustomer(SettingService.GetEmailAccount(), emailTemplate);
                PaymentLogger.Info("Order confirmation email sent to customer.");
            }
            catch (Exception e)
            {
                PaymentLogger.Error($"Failed to send order confirmation email: {e.Message}", e);
            }

            try
            {
                var emailTemplate = RazorEngineHelper.CompanyGotNewOrderEmail(order.Id);
                if (emailTemplate.Item2.Result == null)
                {
                    PaymentLogger.Error("RazorEngineHelper CompanyGotNewOrderEmail template Is NULL.order.Id:" + order.Id);
                }
                PaymentLogger.Info("Generated company new order email template.");
                EmailSender.SendRenderedEmailTemplateToAdminUsers(SettingService.GetEmailAccount(), emailTemplate);
                PaymentLogger.Info("New order email sent to admin users.");
            }
            catch (Exception e)
            {
                PaymentLogger.Error($"Failed to send company new order email: {e.Message}", e);
            }
        }

        public ActionResult ShoppingWithoutAccount()
        {
            ShoppingCartSession shoppingCart = GetShoppingCart();
            ViewBag.ShoppingCartSession = shoppingCart;
            var buyWithNoAccountCreation = new BuyWithNoAccountCreation();
            buyWithNoAccountCreation.ShoppingCartItems = shoppingCart.ShoppingCartItems;
            buyWithNoAccountCreation.Coupon = shoppingCart.Coupon;
            buyWithNoAccountCreation.Customer = shoppingCart.Customer;
            PaymentLogger.Info("Returning ShoppingWithoutAccount view.");
            return View(buyWithNoAccountCreation);
        }

        [HttpPost]
        public ActionResult ShoppingWithoutAccount(Customer customer)
        {
            PaymentLogger.Info("Entering ContinueShoppingWithoutAccount POST action.");
            if (customer == null)
            {
                PaymentLogger.Error("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }
            ShoppingCartSession shoppingCart = GetShoppingCart();
            ViewBag.ShoppingCartSession = shoppingCart;

            var buyWithNoAccountCreation = new BuyWithNoAccountCreation();
            buyWithNoAccountCreation.OrderGuid = shoppingCart.OrderGuid;
            buyWithNoAccountCreation.ShoppingCartItems = shoppingCart.ShoppingCartItems;
            buyWithNoAccountCreation.Coupon = shoppingCart.Coupon;
            bool isValidCustomer = customer.isValidCustomer();
            PaymentLogger.Info($"Customer validation result: {isValidCustomer}");
            if (isValidCustomer)
            {
                PaymentLogger.Info("Saving customer information");

                customer.CustomerType = (int)EImeceCustomerType.ShoppingWithoutAccount;
                customer.Country = Domain.Constants.IYZICO_ADDRESS_COUNTRY;
                customer.Ip = GeneralHelper.GetIpAddress();
                customer.CreatedDate = DateTime.Now;
                customer.UpdatedDate = DateTime.Now;
                customer.GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber);
                customer.UserId = Guid.NewGuid().ToString();
                customer = CustomerService.SaveOrEditEntity(customer);
                PaymentLogger.Info("Saving customer information,customer.Id:"+ customer.Id);

                shoppingCart.Customer = customer;
                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                PaymentLogger.Info("Set shipping and billing addresses.");

                ShoppingCart item = SaveShoppingCart(shoppingCart);
          

                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitialize(shoppingCart, item.UserId, "ShoppingWithoutAccountResult");
                return View("ShoppingWithoutAccountPayment", buyWithNoAccountCreation);
            }
            else
            {
                InformCustomerToFillOutForm(customer);
                shoppingCart.Customer = customer;
                PaymentLogger.Info("Returning view with validation errors.");
                return View(buyWithNoAccountCreation);
            }
        }

        public ActionResult ShoppingWithoutAccountResult(RetrieveCheckoutFormRequest model, String o, String orderNumber)
        {
            PaymentLogger.Info("Entering BuyNowPaymentResult action.");
            CheckoutForm checkoutForm = IyzicoService.GetCheckoutForm(model);
            PaymentLogger.Info($"ShoppingWithoutAccountResult status: {checkoutForm.PaymentStatus} ConversationId: {checkoutForm.ConversationId}");

            PaymentLogger.Info("ShoppingWithoutAccountResult.CheckoutForm: " + JsonConvert.SerializeObject(checkoutForm));
            PaymentLogger.Info("ShoppingWithoutAccountResult.RetrieveCheckoutFormRequest: " + JsonConvert.SerializeObject(model));
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                PaymentLogger.Info($"Decrypted orderGuid: {orderGuid}");
                var item = ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid);
                ShoppingCartSession shoppingCart = GetShoppingCartByOrderGuid(orderGuid);
                BuyWithNoAccountCreation buyWithNoAccountCreation = JsonConvert.DeserializeObject<BuyWithNoAccountCreation>(item.ShoppingCartJson);
                PaymentLogger.Info("Deserialized BuyWithNoAccountCreation model from shopping cart.");
                if (buyWithNoAccountCreation.ShoppingCartItems.IsEmpty())
                {
                    PaymentLogger.Error("ShoppingCartItem or Product is null in buyWithNoAccountCreation model.");
                    throw new ArgumentException("buyWithNoAccountCreation.ShoppingCartItem.ShoppingCartItems cannot be empty");
                }
                if (buyWithNoAccountCreation.Customer == null)
                {
                    PaymentLogger.Error("Customer is null in BuyNow model.");
                    throw new ArgumentException("buyWithNoAccountCreation.Customer cannot be null");
                }
                buyWithNoAccountCreation.CargoCompany = SettingService.GetSettingObjectByKey(Domain.Constants.CargoCompany);
                buyWithNoAccountCreation.BasketMinTotalPriceForCargo = SettingService.GetSettingObjectByKey(Domain.Constants.BasketMinTotalPriceForCargo);
                buyWithNoAccountCreation.CargoPrice = SettingService.GetSettingObjectByKey(Domain.Constants.CargoPrice);
                buyWithNoAccountCreation.Customer.Lang = CurrentLanguage;
                PaymentLogger.Info("Updated buyWithNoAccountCreation model with cargo and language details.");

                var order = ShoppingCartService.SaveBuyWithNoAccountCreation(orderNumber, buyWithNoAccountCreation, checkoutForm);
                PaymentLogger.Info($"Order saved with ID: {order.Id}");
                SendNotificationEmailsToCustomerAndAdminUsersForNewOrder(OrderService.GetOrderById(order.Id));
                ClearBuyWithNoAccountCreation(buyWithNoAccountCreation);
                ClearCart(shoppingCart);
                PaymentLogger.Info("Cleared buyWithNoAccountCreation cart. Redirecting to ThankYouForYourOrder.");
                return RedirectToAction("ThankYouForYourOrder", new { orderId = order.Id });
            }
            else
            {
                PaymentLogger.Error($"BuyWithNoAccountCreation payment failed. CheckoutForm: {JsonConvert.SerializeObject(checkoutForm)}");
                PaymentLogger.Info("Redirecting to NoSuccessForYourOrder.");
                return RedirectToAction("NoSuccessForYourOrder");
            }
        }

        private void ClearBuyWithNoAccountCreation(BuyWithNoAccountCreation buyWithNoAccountCreation)
        {
            PaymentLogger.Info($"Entering ClearBuyWithNoAccountCreation with OrderGuid: {buyWithNoAccountCreation.OrderGuid}");
            ShoppingCartService.DeleteByOrderGuid(buyWithNoAccountCreation.OrderGuid);
            PaymentLogger.Info("BuyNow cart deleted from data source.");
        }
    }
}