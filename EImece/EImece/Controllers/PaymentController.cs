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
    public class PaymentController : BaseController
    {
        private const string CUSTOMER_COUNTRY = "Türkiye";
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
            PaymentLogger.Info("PaymentController constructor called. Initializing UserManager and SignInManager.");
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ActionResult ShoppingCart()
        {
            PaymentLogger.Info("Entering ShoppingCart action.");
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
            PaymentLogger.Info("Entering Index action.");
            PaymentLogger.Info("Returning Index view.");
            return View();
        }

        [ChildActionOnly]
        public ActionResult HomePageShoppingCart()
        {
            return PartialView("ShoppingCartTemplates/_HomePageShoppingCart", GetShoppingCart());
        }

        public ActionResult AddToCart(string productId, int quantity, string orderGuid, string productSpecItems)
        {
            PaymentLogger.Info($"Entering AddToCart action with productId: {productId}, quantity: {quantity}, orderGuid: {orderGuid}");
            int pId = GeneralHelper.RevertId(productId);
            PaymentLogger.Info($"Reverted productId to: {pId}");
            var product = ProductService.GetProductById(pId);
            if (product != null)
            {
                PaymentLogger.Info($"Product found with ID: {pId}");
                var shoppingCart = GetShoppingCart();
                shoppingCart.OrderGuid = orderGuid;
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
                PaymentLogger.Info("Shopping cart saved successfully.");
                PaymentLogger.Info("Returning success JSON response.");
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                PaymentLogger.Error($"Product not found with ID: {pId}");
                PaymentLogger.Info("Returning failed JSON response.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [NoCache]
        public ActionResult GetShoppingCartSmallDetails()
        {
            PaymentLogger.Info("Entering GetShoppingCartSmallDetails action.");
            var shoppingCart = GetShoppingCartFromDataSource();
            PaymentLogger.Info("Retrieved shopping cart from data source.");
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\ShoppingCartTemplates\_ShoppingCartSmallDetails.cshtml",
                        new ViewDataDictionary(shoppingCart), tempData);
            PaymentLogger.Info("Rendered shopping cart small details HTML.");
            PaymentLogger.Info("Returning JSON response with HTML.");
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [NoCache]
        public ActionResult GetShoppingCartLinks()
        {
            PaymentLogger.Info("Entering GetShoppingCartLinks action.");
            var shoppingCart = GetShoppingCartFromDataSource();
            PaymentLogger.Info("Retrieved shopping cart from data source.");
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\ShoppingCartTemplates\_ShoppingCartLinks.cshtml",
                        new ViewDataDictionary(shoppingCart), tempData);
            PaymentLogger.Info("Rendered shopping cart links HTML.");
            PaymentLogger.Info("Returning JSON response with HTML.");
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        [NoCache]
        public ActionResult ShoppingCartLink()
        {
            PaymentLogger.Info("Entering ShoppingCartLink partial action.");
            var shoppingCart = GetShoppingCartFromDataSource();
            PaymentLogger.Info("Retrieved shopping cart from data source.");
            PaymentLogger.Info("Rendering _ShoppingCartLinks partial view.");
            return PartialView("ShoppingCartTemplates/_ShoppingCartLinks", shoppingCart);
        }

        private void SaveShoppingCart(ShoppingCartSession shoppingCart)
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
        }

        private string getUserId()
        {
            PaymentLogger.Info("Entering getUserId method.");
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
            PaymentLogger.Info("Request is not authenticated or no user ID found. Returning empty string.");
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
            PaymentLogger.Info($"Entering GetShoppingCartByOrderGuid with orderGuid: {orderGuid}");
            ShoppingCartSession result = null;
            var item = orderGuid != null ? ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid) : null;
            if (item == null)
            {
                PaymentLogger.Info("No existing shopping cart found. Creating default shopping cart.");
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
                PaymentLogger.Info($"Created default shopping cart with IP: {GeneralHelper.GetIpAddress()}");
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
            PaymentLogger.Info("Entering GetCustomerIfAuthenticated method.");
            if (Request.IsAuthenticated)
            {
                PaymentLogger.Info("Request is authenticated.");
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
                    PaymentLogger.Info("Customer assigned to shopping cart.");
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
            PaymentLogger.Info("Entering CheckoutBillingDetails action.");
            if (Request.IsAuthenticated)
            {
                PaymentLogger.Info("User is authenticated.");
                ShoppingCartSession shoppingCart = GetShoppingCart();
                if (shoppingCart.ShoppingCartItems.IsNotEmpty())
                {
                    PaymentLogger.Info("Shopping cart has items.");
                    if (shoppingCart.Customer == null)
                    {
                        PaymentLogger.Info("No customer in shopping cart. Creating new customer.");
                        shoppingCart.Customer = new Customer();
                        shoppingCart.Customer.CustomerType = (int)EImeceCustomerType.Normal;
                        shoppingCart.Customer.Country = CUSTOMER_COUNTRY;
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
                PaymentLogger.Info("Returning view with validation errors.");
                return View(shoppingCart);
            }
        }
  
        public ActionResult CheckoutDelivery()
        {
            PaymentLogger.Info("Entering CheckoutDelivery action.");
            ShoppingCartSession shoppingCart = GetShoppingCart();
            PaymentLogger.Info("Returning CheckoutDelivery view.");
            return View(shoppingCart);
        }

        public ActionResult CheckoutPaymentOrderReview()
        {
            PaymentLogger.Info("Entering CheckoutPaymentOrderReview action.");
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
            PaymentLogger.Info("Shopping cart saved with order comments.");
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
                PaymentLogger.Info("Returning failed JSON response.");
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
                PaymentLogger.Info("Shopping cart saved after removal.");
                PaymentLogger.Info("Returning success JSON response.");
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
                var user = UserManager.FindByName(User.Identity.GetUserName());
                PaymentLogger.Info($"Initializing checkout form for user ID: {user.Id}");
                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitialize(shoppingCart, user.Id);
                PaymentLogger.Info("Returning PlaceOrder view.");
                return View(shoppingCart);
            }
            else
            {
                PaymentLogger.Info("Customer validation failed or cart is empty. Returning RegisterCustomer content.");
                return Content("RegisterCustomer");
            }
        }
         

        public ActionResult PaymentResult(RetrieveCheckoutFormRequest model, string o, string u)
        {
            PaymentLogger.Info("Entering PaymentResult action.");
            var checkoutForm = IyzicoService.GetCheckoutForm(model);
            PaymentLogger.Info($"Payment status: {checkoutForm.PaymentStatus}");
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                PaymentLogger.Info($"Decrypted orderGuid: {orderGuid}");
                ShoppingCartSession shoppingCart = GetShoppingCartByOrderGuid(orderGuid);
                var userId = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(u));
                PaymentLogger.Info($"Decrypted userId: {userId}");
                var order = ShoppingCartService.SaveShoppingCart(shoppingCart, checkoutForm, userId);
                PaymentLogger.Info($"Order saved with ID: {order.Id}");
                SendNotificationEmailsToCustomerAndAdminUsersForNewOrder(OrderService.GetOrderById(order.Id));
                ClearCart(shoppingCart);
                PaymentLogger.Info("Cart cleared. Redirecting to ThankYouForYourOrder.");
                return RedirectToAction("ThankYouForYourOrder", new { orderId = order.Id });
            }
            else
            {
                PaymentLogger.Error($"Payment failed. CheckoutForm: {JsonConvert.SerializeObject(checkoutForm)}");
                PaymentLogger.Info("Redirecting to NoSuccessForYourOrder.");
                return RedirectToAction("NoSuccessForYourOrder");
            }
        }

        public ActionResult ThankYouForYourOrder(int orderId)
        {
            PaymentLogger.Info($"Entering ThankYouForYourOrder with orderId: {orderId}");
            var order = OrderService.GetOrderById(orderId);
            //SendNotificationEmailsToCustomerAndAdminUsersForNewOrder(OrderService.GetOrderById(order.Id));
            PaymentLogger.Info("Returning ThankYouForYourOrder view.");
            return View(order);
        }

        public ActionResult NoSuccessForYourOrder()
        {
            PaymentLogger.Info("Entering NoSuccessForYourOrder action.");
            PaymentLogger.Info("Returning NoSuccessForYourOrder view.");
            return View();
        }

        private void ClearCart(ShoppingCartSession shoppingCart)
        {
            PaymentLogger.Info("Entering ClearCart method.");
            if (Request.Browser.Cookies)
            {
                PaymentLogger.Info("Removing OrderGuid cookie.");
                Response.Cookies.Remove(Domain.Constants.OrderGuidCookieKey);
                var aCookie = new HttpCookie(Domain.Constants.OrderGuidCookieKey) { Expires = DateTime.Now.AddDays(-1) };
                Response.Cookies.Add(aCookie);
                PaymentLogger.Info("Added expired cookie to response.");
            }
            ShoppingCartService.DeleteByOrderGuid(shoppingCart.OrderGuid);
            PaymentLogger.Info($"Deleted shopping cart with OrderGuid: {shoppingCart.OrderGuid}");
        }

        public ActionResult PaymentSuccess(RetrieveCheckoutFormRequest model)
        {
            PaymentLogger.Info("Entering PaymentSuccess action.");
            PaymentLogger.Info("Returning PaymentSuccess content.");
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
            var checkoutForm = IyzicoService.GetCheckoutForm(model);
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
                PaymentLogger.Error("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }
            if (string.IsNullOrEmpty(customer.Name.ToStr().Trim()))
            {
                PaymentLogger.Info("Name is empty. Adding model error.");
                ModelState.AddModelError("customer.Name", Resource.PleaseEnterYourName);
            }
            if (string.IsNullOrEmpty(customer.Surname.ToStr().Trim()))
            {
                PaymentLogger.Info("Surname is empty. Adding model error.");
                ModelState.AddModelError("customer.Surname", Resource.PleaseEnterYourSurname);
            }
            if (string.IsNullOrEmpty(customer.GsmNumber.ToStr().Trim()))
            {
                PaymentLogger.Info("GsmNumber is empty. Adding model error.");
                ModelState.AddModelError("customer.GsmNumber", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Email.ToStr().Trim()))
            {
                PaymentLogger.Info("Email is empty. Adding model error.");
                ModelState.AddModelError("customer.Email", Resource.PleaseEnterYourEmail);
            }
            if (string.IsNullOrEmpty(customer.City.ToStr().Trim()))
            {
                PaymentLogger.Info("City is empty. Adding model error.");
                ModelState.AddModelError("customer.City", Resource.PleaseEnterYourCity);
            }
            if (string.IsNullOrEmpty(customer.Town.ToStr().Trim()))
            {
                PaymentLogger.Info("Town is empty. Adding model error.");
                ModelState.AddModelError("customer.Town", Resource.PleaseEnterYourTown);
            }
            if (string.IsNullOrEmpty(customer.Country.ToStr().Trim()))
            {
                PaymentLogger.Info("Country is empty. Adding model error.");
                ModelState.AddModelError("customer.Country", Resource.PleaseEnterYourCountry);
            }
            if (string.IsNullOrEmpty(customer.District.ToStr().Trim()))
            {
                PaymentLogger.Info("District is empty. Adding model error.");
                ModelState.AddModelError("customer.District", Resource.PleaseEnterYourDistrict);
            }
            if (string.IsNullOrEmpty(customer.Street.ToStr().Trim()))
            {
                PaymentLogger.Info("Street is empty. Adding model error.");
                ModelState.AddModelError("customer.Street", Resource.PleaseEnterYourStreet);
            }
           // if (string.IsNullOrEmpty(customer.IdentityNumber.ToStr().Trim()))
           // {
           //     PaymentLogger.Info("IdentityNumber is empty. Adding model error.");
           //     ModelState.AddModelError("customer.IdentityNumber", Resource.MandatoryField);
           // }
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
                if(emailTemplate.Item2.Result == null)
                {
                    PaymentLogger.Error("RazorEngineHelper OrderConfirmationEmail template Is NULL.order.Id:"+ order.Id);
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
            var p = new BuyWithNoAccountCreation();
            p.ShoppingCartItems = shoppingCart.ShoppingCartItems;
            PaymentLogger.Info("Returning ShoppingWithoutAccount view.");
            return View(p);
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
            var p = new BuyWithNoAccountCreation();
            p.ShoppingCartItems = shoppingCart.ShoppingCartItems;
            bool isValidCustomer = customer.isValidCustomer();
            PaymentLogger.Info($"Customer validation result: {isValidCustomer}");
            if (isValidCustomer)
            {
                customer.CustomerType = (int)EImeceCustomerType.ShoppingWithoutAccount;
                shoppingCart.Customer = customer;
                shoppingCart.Customer.Country = CUSTOMER_COUNTRY;
                shoppingCart.Customer.Ip = GeneralHelper.GetIpAddress();

                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                PaymentLogger.Info("Set shipping and billing addresses.");

                SaveShoppingCart(shoppingCart);

                p.OrderGuid = Guid.NewGuid().ToString();
                PaymentLogger.Info($"Set shipping address and OrderGuid: {p.OrderGuid}");

                var item = new ShoppingCart();
                item.CreatedDate = DateTime.Now;
                item.UpdatedDate = DateTime.Now;
                item.Name = p.OrderGuid;
                item.IsActive = false;
                item.Lang = CurrentLanguage;
                item.Position = 0;
                item.ShoppingCartJson = JsonConvert.SerializeObject(p);
                item.OrderGuid = p.OrderGuid;
                item.UserId = Domain.Constants.ShoppingWithoutAccountUserId;
                ShoppingCartService.SaveOrEditShoppingCart(item);
                PaymentLogger.Info("Saved ShoppingWithoutAccount shopping cart.");
           
                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitialize(shoppingCart, item.UserId, "ShoppingWithoutAccountResult");
                return View("ShoppingWithoutAccountPayment", p);
            }
            else
            {
                InformCustomerToFillOutForm(customer);
                shoppingCart.Customer = customer;
                PaymentLogger.Info("Returning view with validation errors.");
                return View(p);
            }
        }

        public ActionResult ShoppingWithoutAccountResult(RetrieveCheckoutFormRequest model, String o)
        {
            PaymentLogger.Info("Entering BuyNowPaymentResult action.");
            var checkoutForm = IyzicoService.GetCheckoutForm(model);
            PaymentLogger.Info($"Payment status: {checkoutForm.PaymentStatus}");
            if (checkoutForm.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase))
            {
                var orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                PaymentLogger.Info($"Decrypted orderGuid: {orderGuid}");
                var item = ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid);
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

                var order = ShoppingCartService.SaveBuyWithNoAccountCreation(buyWithNoAccountCreation, checkoutForm);
                PaymentLogger.Info($"Order saved with ID: {order.Id}");
                ClearBuyWithNoAccountCreation(buyWithNoAccountCreation);
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