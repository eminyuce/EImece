using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using ProductSpecItem = EImece.Domain.Models.FrontModels.ProductSpecItem;
using EImece.Domain.Models.Payment;
using PaymentResultDto = EImece.Domain.Models.Payment.PaymentResult;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Services;
using EImece.Domain.Services.Payment;
using EImece.Domain.Services.IServices;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using EImece.Domain.DependencyInjection;
using EImece.Filters;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string ShoppingCartAction = "shoppingcart";
        private const string LastCompletedOrderIdKey = "LastCompletedOrderId";
        private const string ThankYouForYourOrderAction = "ThankYouForYourOrder";

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        [Inject]
        public ICouponService CouponService { get; set; }

        [Inject]
        public IRazorEngineHelper RazorEngineHelper { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public IAddressService AddressService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public PaymentContext PaymentContext { get; set; }

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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IsProductPriceEnabled)
            {
                if (filterContext.IsChildAction)
                {
                    filterContext.Result = new ContentResult { Content = string.Empty };
                    return;
                }

                filterContext.Result = RedirectToAction("Index", "Home");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        public async Task<ActionResult> ShoppingCart()
        {
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                PaymentLogger.Info($"Setting UrlReferrer to: {urlReferrer}");
                shoppingCart.UrlReferrer = urlReferrer.ToStr();
            }
            PaymentLogger.Info("Returning ShoppingCart view.");
            return View(shoppingCart);
        }

        [ChildActionOnly]
        public ActionResult HomePageShoppingCart()
        {
            return PartialView("ShoppingCartTemplates/_HomePageShoppingCart", GetShoppingCartSync());
        }

        [HttpPost]
        public async Task<JsonResult> AddToCart(string productId, int quantity, string orderGuid, string productSpecItems)
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
            var product = await ProductService.GetProductByIdAsync(pId);
            if (product != null)
            {
                PaymentLogger.Info($"Product found with ID: {pId}");
                var shoppingCart = await GetShoppingCartAsync();
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
                await SaveShoppingCartAsync(shoppingCart);
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
        public async Task<JsonResult> GetShoppingCartSmallDetails()
        {
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        "ShoppingCartTemplates/_ShoppingCartSmallDetails",
                        new ViewDataDictionary(shoppingCart), tempData);
            PaymentLogger.Info("Returning JSON response with HTML.");
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [NoCache]
        public async Task<JsonResult> GetShoppingCartLinks()
        {
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        "ShoppingCartTemplates/_ShoppingCartLinks",
                        new ViewDataDictionary(shoppingCart), tempData);
            PaymentLogger.Info("Returning JSON response with HTML.");
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        [NoCache]
        public ActionResult ShoppingCartLink()
        {
            if (!IsProductPriceEnabled)
            {
                return Content(string.Empty);
            }
            var shoppingCart = GetShoppingCartFromDataSourceSync();
            PaymentLogger.Info("Rendering _ShoppingCartLinks partial view.");
            return PartialView("ShoppingCartTemplates/_ShoppingCartLinks", shoppingCart);
        }

        private ShoppingCartSession GetShoppingCartSync()
        {
            return GetShoppingCartFromDataSourceSync();
        }

        private ShoppingCartSession GetShoppingCartFromDataSourceSync()
        {
            HttpCookie orderGuid = Request.Cookies[Domain.Constants.OrderGuidCookieKey];
            string orderGuid2 = orderGuid == null ? null : orderGuid.Value;
            return GetShoppingCartByOrderGuidSync(orderGuid2);
        }

        private ShoppingCartSession GetShoppingCartByOrderGuidSync(string orderGuid)
        {
            ShoppingCartSession result = null;
            var item = orderGuid != null ? ShoppingCartService.GetShoppingCartByOrderGuid(orderGuid) : null;
            if (item == null)
            {
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
            }
            else
            {
                result = JsonConvert.DeserializeObject<ShoppingCartSession>(item.ShoppingCartJson);
            }

            result.CargoCompany = SettingService.GetSettingValueDtoByKey(Domain.Constants.CargoCompany);
            result.BasketMinTotalPriceForCargo = SettingService.GetSettingValueDtoByKey(Domain.Constants.BasketMinTotalPriceForCargo);
            result.CargoPrice = SettingService.GetSettingValueDtoByKey(Domain.Constants.CargoPrice);
            return result;
        }

        private async Task<ShoppingCart> SaveShoppingCartAsync(ShoppingCartSession shoppingCart)
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
            item.UserId = string.IsNullOrEmpty(userId) ? await getUserIdAsync() : userId;
            PaymentLogger.Info($"Saving shopping cart with OrderGuid: {item.OrderGuid}, UserId: {item.UserId}");

            shoppingCart.CurrentLanguage = CurrentLanguage;
            await ShoppingCartService.SaveOrEditShoppingCartAsync(item);
            PaymentLogger.Debug("Shopping cart saved to data source.");

            return item;
        }

        private async Task<string> getUserIdAsync()
        {
            if (Request.IsAuthenticated)
            {
                PaymentLogger.Info("Request is authenticated.");
                var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                if (user != null)
                {
                    PaymentLogger.Info($"User found with ID: {user.Id}");
                    return user.Id;
                }
                PaymentLogger.Info("No user found.");
            }
            return string.Empty;
        }

        private async Task<ShoppingCartSession> GetShoppingCartFromDataSourceAsync()
        {
            PaymentLogger.Info("Entering GetShoppingCartFromDataSourceAsync method.");
            HttpCookie orderGuid = Request.Cookies[Domain.Constants.OrderGuidCookieKey];
            string orderGuid2 = orderGuid == null ? null : orderGuid.Value;
            PaymentLogger.Info($"Retrieved OrderGuid from cookie: {orderGuid2}");
            var result = await GetShoppingCartByOrderGuidAsync(orderGuid2);
            PaymentLogger.Debug("Shopping cart retrieved from GetShoppingCartByOrderGuidAsync.");
            return result;
        }

        private async Task<ShoppingCartSession> GetShoppingCartByOrderGuidAsync(string orderGuid)
        {
            ShoppingCartSession result = null;
            var item = orderGuid != null ? await ShoppingCartService.GetShoppingCartByOrderGuidAsync(orderGuid) : null;
            if (item == null)
            {
                PaymentLogger.Info("No existing shopping cart found. Creating default shopping cart.");
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
                await GetCustomerIfAuthenticatedAsync(result);
            }
            else
            {
                PaymentLogger.Info("Existing shopping cart found. Deserializing JSON.");
                result = JsonConvert.DeserializeObject<ShoppingCartSession>(item.ShoppingCartJson);
                string userId = result.Customer != null ? result.Customer.UserId : "";
                item.UserId = string.IsNullOrEmpty(userId) ? await getUserIdAsync() : userId;
                PaymentLogger.Info($"Updated shopping cart UserId to: {item.UserId}");
            }

            result.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            result.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            result.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            PaymentLogger.Info("Set cargo details for shopping cart.");
            return result;
        }

        private async Task GetCustomerIfAuthenticatedAsync(ShoppingCartSession result)
        {
            if (!Request.IsAuthenticated)
            {
                PaymentLogger.Info("Request is not authenticated. No customer assigned.");
                return;
            }

            var userName = User.Identity.GetUserName();
            if (string.IsNullOrWhiteSpace(userName))
            {
                PaymentLogger.Info("Authenticated identity has no user name. No customer assigned.");
                return;
            }

            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                PaymentLogger.Warn("No AspNet user for authenticated name '{0}'. Skipping customer assignment.", userName);
                return;
            }

            PaymentLogger.Info($"User found with ID: {user.Id}");
            var c = await CustomerService.GetUserIdAsync(user.Id);
            if (c == null)
            {
                PaymentLogger.Info("No customer found. Creating new customer.");
                var newCustomer = new CustomerDto();
                newCustomer.UserId = user.Id;
                newCustomer.CustomerType = (int)EImeceCustomerType.Normal;
                newCustomer.IsSameAsShippingAddress = true;
                result.Customer = newCustomer;
            }
            else
            {
                var cDto = Mapper.Map<CustomerDto>(c);
                cDto.IsSameAsShippingAddress = true;
                result.Customer = cDto;
            }
        }

        private async Task<ShoppingCartSession> GetShoppingCartAsync()
        {
            PaymentLogger.Info("Entering GetShoppingCartAsync method.");
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            PaymentLogger.Info("Shopping cart retrieved.");
            return shoppingCart;
        }

        public async Task<ActionResult> CheckoutBillingDetails()
        {
            if (Request.IsAuthenticated)
            {
                ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
                if (shoppingCart.ShoppingCartItems.IsNotEmpty())
                {
                    PaymentLogger.Info("Shopping cart has items.");
                    if (shoppingCart.Customer == null)
                    {
                        PaymentLogger.Info("No customer in shopping cart. Creating new customer.");
                        shoppingCart.Customer = new CustomerDto();
                        shoppingCart.Customer.CustomerType = (int)EImeceCustomerType.Normal;
                        shoppingCart.Customer.Country = Domain.Constants.IYZICO_ADDRESS_COUNTRY;
                        shoppingCart.Customer.Ip = GeneralHelper.GetIpAddress();
                    }
                    if (shoppingCart.Customer.IsEmpty())
                    {
                        PaymentLogger.Info("Customer is empty. Populating from authenticated user.");
                        await GetCustomerIfAuthenticatedAsync(shoppingCart);
                    }
                    PaymentLogger.Info("Returning CheckoutBillingDetails view.");
                    return View(shoppingCart);
                }
                else
                {
                    PaymentLogger.Info("Shopping cart is empty. Redirecting to shoppingcart.");
                    TempData["StatusMessage"] = "Sepetiniz boş";
                    return RedirectToAction(ShoppingCartAction, Domain.Constants.PaymentAction);
                }
            }
            else
            {
                // Membership checkout requires an account + address; send guests to register
                // (not login) so they can create a profile before billing details.
                PaymentLogger.Info("User is not authenticated. Redirecting to register for membership checkout.");
                return RedirectToAction("Register", "Account",
                    new { returnUrl = Url.Action("CheckoutBillingDetails", Domain.Constants.PaymentAction) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckoutBillingDetails(CustomerDto customer)
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
                ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
                customer.CustomerType = (int)EImeceCustomerType.Normal;
                shoppingCart.Customer = customer;
                var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
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

                await SaveShoppingCartAsync(shoppingCart);
                PaymentLogger.Info("Shopping cart saved with billing details.");
                PaymentLogger.Info("Redirecting to CheckoutPaymentOrderReview.");
                return RedirectToAction("CheckoutPaymentOrderReview");
            }
            else
            {
                PaymentLogger.Info("Customer validation failed. Informing customer.");
                InformCustomerToFillOutForm(customer);
                ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
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

            orderNumber = orderNumber.Trim();
            if (orderNumber.Length > 50)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

            var order = await OrderService.GetByOrderNumberAsync(orderNumber);
            if (order == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var orderDto = Mapper.Map<OrderDto>(order);
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        "CargoTrackingResult",
                        new ViewDataDictionary(orderDto), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CheckoutPaymentOrderReview()
        {
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            if (shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                PaymentLogger.Info("Shopping cart has items. Returning view.");
                return View(shoppingCart);
            }
            else
            {
                PaymentLogger.Info("Shopping cart is empty. Redirecting to shoppingcart.");
                return RedirectToAction(ShoppingCartAction, Domain.Constants.PaymentAction);
            }
        }

        public async Task<JsonResult> renderShoppingCartPrice()
        {
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
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

        public async Task<JsonResult> sendOrderComments(string orderComments, string orderGuid)
        {
            PaymentLogger.Info($"Entering sendOrderComments with orderComments: {orderComments}, orderGuid: {orderGuid}");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            shoppingCart.OrderComments = orderComments;
            PaymentLogger.Info("Order comments assigned to shopping cart.");
            await SaveShoppingCartAsync(shoppingCart);
            PaymentLogger.Info("Returning success JSON response.");
            return Json(new { status = Domain.Constants.SUCCESS }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> UpdateQuantity(String shoppingItemId, int quantity)
        {
            PaymentLogger.Info($"Entering UpdateQuantity with shoppingItemId: {shoppingItemId}, quantity: {quantity}");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                PaymentLogger.Info($"Found item with ID: {shoppingItemId}. Updating quantity to: {quantity}");
                item.Quantity = quantity;
                await SaveShoppingCartAsync(shoppingCart);
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

        public async Task<JsonResult> RemoveCart(String shoppingItemId)
        {
            PaymentLogger.Info($"Entering RemoveCart with shoppingItemId: {shoppingItemId}");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
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
                await SaveShoppingCartAsync(shoppingCart);
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                PaymentLogger.Error($"Item with ID: {shoppingItemId} not found.");
                PaymentLogger.Info("Returning failed JSON response.");
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
        }

        [RateLimit("checkout", DefaultLimit = 5, DefaultWindowMinutes = 5)]
        public async Task<ActionResult> PlaceOrder()
        {
            PaymentLogger.Info("Entering PlaceOrder action.");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();

            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsEmpty())
            {
                PaymentLogger.Info("Shopping cart is null or empty. Redirecting to shoppingcart.");
                return RedirectToAction(ShoppingCartAction, Domain.Constants.PaymentAction);
            }
            if (shoppingCart.Customer.isValidCustomer() && shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                PaymentLogger.Info("Customer is valid and cart has items.");
                if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    await RevalidateCouponAsync(shoppingCart);
                    await SaveShoppingCartAsync(shoppingCart);
                    var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                    PaymentLogger.Info($"Initializing checkout form for user ID: {user.Id} via {PaymentContext.ProviderName}");
                    try
                    {
                        if (!AppConfig.HasConfiguredIyzicoCredentials)
                        {
                            PaymentLogger.Warn("Iyzico API keys are empty; skipping checkout form initialize.");
                            ViewBag.CheckoutFormInitialize = new PaymentInitializeResult
                            {
                                ErrorMessage = Resource.PaymentFormCouldNotBeInitializedConfig,
                                ProviderName = PaymentContext.ProviderName
                            };
                            ModelState.AddModelError("", Resource.PaymentFormCouldNotBeInitializedConfig);
                        }
                        else
                        {
                            ViewBag.CheckoutFormInitialize = await PaymentContext.InitializeCheckoutAsync(shoppingCart, user.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        PaymentLogger.Error(ex, "Failed to initialize payment checkout form via {0}.", PaymentContext.ProviderName);
                        ViewBag.CheckoutFormInitialize = null;
                        ModelState.AddModelError("", Resource.PaymentFormCouldNotBeInitializedSettings);
                    }
                    PaymentLogger.Info("Returning PlaceOrder view.");
                    return View(shoppingCart);
                }
            }
            else
            {
                return Content("RegisterCustomer");
            }
        }
        
        public async Task<ActionResult> PaymentResult(PaymentCallbackRequest model, string o, string u, String orderNumber)
        {
            // iyzico Checkout Form callback POSTs "token" — same binding as the former RetrieveCheckoutFormRequest.
            PaymentResultDto paymentResult = await PaymentContext.RetrievePaymentResultAsync(model != null ? model.Token : null);
            PaymentLogger.Info($"PaymentResult with ACCOUNT status: {paymentResult.PaymentStatus} ConversationId: {paymentResult.ConversationId}");
            if (!IsSuccessfulPayment(paymentResult))
            {
                PaymentLogger.Error($"Payment failed. Status: {paymentResult?.PaymentStatus}");
                return RedirectToAction("NoSuccessForYourOrder");
            }

            string orderGuid;
            string userId;
            try
            {
                orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
                userId = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(u));
            }
            catch (Exception ex)
            {
                PaymentLogger.Error(ex, "Failed to decrypt payment callback references.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var bindingError = ValidatePaymentBinding(paymentResult, orderGuid, orderNumber);
            if (bindingError != null)
            {
                return bindingError;
            }

            var existingOrder = await FindExistingPaidOrderAsync(paymentResult.PaymentId, orderGuid);
            if (existingOrder != null)
            {
                TempData[LastCompletedOrderIdKey] = existingOrder.Id;
                return RedirectToAction(ThankYouForYourOrderAction, new { orderId = existingOrder.Id });
            }

            ShoppingCartSession shoppingCart = await GetShoppingCartByOrderGuidAsync(orderGuid);
            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsEmpty())
            {
                PaymentLogger.Error($"Shopping cart missing for OrderGuid after successful payment.");
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }

            if (!PaidPriceMatches(paymentResult.PaidPrice, shoppingCart.TotalPriceWithCargoPrice))
            {
                PaymentLogger.Error($"PaidPrice mismatch for OrderGuid. Expected cart total does not match payment.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var resolvedOrderNumber = string.IsNullOrWhiteSpace(paymentResult.ConversationId)
                ? orderNumber
                : paymentResult.ConversationId;
            var order = await ShoppingCartService.SaveShoppingCartAsync(resolvedOrderNumber, shoppingCart, paymentResult, userId);
            PaymentLogger.Info($"Order saved with ID: {order.Id}. Cart cleared. Redirecting to ThankYouForYourOrder.");
            await SendNotificationEmailsToCustomerAndAdminUsersForNewOrderAsync(await OrderService.GetOrderByIdAsync(order.Id));
            await ClearCartAsync(shoppingCart);
            PaymentLogger.Debug("Cart cleared. Redirecting to ThankYouForYourOrder.");
            TempData[LastCompletedOrderIdKey] = order.Id;
            return RedirectToAction(ThankYouForYourOrderAction, new { orderId = order.Id });
        }

        public async Task<ActionResult> ThankYouForYourOrder(int orderId)
        {
            var order = await OrderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (!CanViewOrder(order))
            {
                return new HttpUnauthorizedResult();
            }

            var orderDto = Mapper.Map<OrderDto>(order);
            return View(orderDto);
        }

        private bool CanViewOrder(Order order)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                return !string.IsNullOrEmpty(order.UserId)
                    && order.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);
            }

            var lastCompletedOrderId = TempData[LastCompletedOrderIdKey] as int?;
            return lastCompletedOrderId.HasValue && lastCompletedOrderId.Value == order.Id;
        }

        public ActionResult NoSuccessForYourOrder()
        {
            return View();
        }

        private async Task ClearCartAsync(ShoppingCartSession shoppingCart)
        {
            PaymentLogger.Info("Entering ClearCartAsync method.");
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
            await ShoppingCartService.DeleteByOrderGuidAsync(shoppingCart.OrderGuid);
            PaymentLogger.Info($"Deleted shopping cart with OrderGuid: {shoppingCart.OrderGuid}");
        }

        public async Task<ActionResult> BuyNow(String id)
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
                BuyNowModel buyNowModel = await CreateBuyNowModelAsync(productId);
                PaymentLogger.Info("Created BuyNow model.");
                ViewBag.SeoId = buyNowModel.ProductDetailViewModel.ProductDto.SeoUrl;
                PaymentLogger.Info($"Set SeoId in ViewBag: {ViewBag.SeoId}");
                PaymentLogger.Info("Returning BuyNow view.");
                return View(buyNowModel);
            }
            catch (Exception e)
            {
                return HandleUnexpectedError(e, $"Exception in BuyNow: {e.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit("checkout", DefaultLimit = 5, DefaultWindowMinutes = 5)]
        public async Task<ActionResult> BuyNow(String productId, CustomerDto customer)
        {
            PaymentLogger.Info($"Entering BuyNow POST with productId: {productId}");
            if (customer == null)
            {
                PaymentLogger.Error("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }

            bool isValidCustomer = customer.isValidCustomer();
            PaymentLogger.Info($"Customer validation result: {isValidCustomer}");
            BuyNowModel buyNowModel = await CreateBuyNowModelAsync(GeneralHelper.RevertId(productId));
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
                await ShoppingCartService.SaveOrEditShoppingCartAsync(item);
                PaymentLogger.Info("Saved BuyNow shopping cart.");

                ViewBag.CheckoutFormInitialize = await PaymentContext.InitializeBuyNowAsync(buyNowModel);
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

        private async Task<BuyNowModel> CreateBuyNowModelAsync(int productId)
        {
            PaymentLogger.Info($"Entering CreateBuyNowModelAsync with productId: {productId}");
            BuyNowModel buyNowModel = new BuyNowModel();
            buyNowModel.ProductId = productId;
            buyNowModel.ProductDetailViewModel = await ProductService.GetProductDetailViewModelByIdAsync(productId);
            PaymentLogger.Debug("Set product details in BuyNow model.");
            buyNowModel.ShoppingCartItem = new ShoppingCartItem();
            var buyNowProduct = await ProductService.GetProductByIdAsync(productId);
            buyNowModel.ShoppingCartItem.Product = new ShoppingCartProduct(buyNowProduct, new List<ProductSpecItem>());
            buyNowModel.ShoppingCartItem.Quantity = 1;
            buyNowModel.ShoppingCartItem.ShoppingCartItemId = Guid.NewGuid().ToString();
            PaymentLogger.Info($"Created shopping cart item with ID: {buyNowModel.ShoppingCartItem.ShoppingCartItemId}");
            buyNowModel.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            buyNowModel.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            buyNowModel.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            PaymentLogger.Debug("Set cargo details in BuyNow model.");
            return buyNowModel;
        }

        public async Task<ActionResult> BuyNowPaymentResult(PaymentCallbackRequest model, String o)
        {
            PaymentResultDto paymentResult = await PaymentContext.RetrievePaymentResultAsync(model != null ? model.Token : null);
            PaymentLogger.Info("Entering BuyNowPaymentResult action. Payment status: {0}", paymentResult.PaymentStatus);
            if (!IsSuccessfulPayment(paymentResult))
            {
                PaymentLogger.Error($"BuyNow payment failed. Status: {paymentResult?.PaymentStatus}");
                return RedirectToAction("NoSuccessForYourOrder");
            }

            string orderGuid;
            try
            {
                orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
            }
            catch (Exception ex)
            {
                PaymentLogger.Error(ex, "Failed to decrypt BuyNow payment callback order reference.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!string.Equals(paymentResult.BasketId, orderGuid, StringComparison.OrdinalIgnoreCase))
            {
                PaymentLogger.Error("BuyNow payment BasketId does not match callback order reference.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var existingOrder = await FindExistingPaidOrderAsync(paymentResult.PaymentId, orderGuid);
            if (existingOrder != null)
            {
                TempData[LastCompletedOrderIdKey] = existingOrder.Id;
                return RedirectToAction(ThankYouForYourOrderAction, new { orderId = existingOrder.Id });
            }

            var item = await ShoppingCartService.GetShoppingCartByOrderGuidAsync(orderGuid);
            if (item == null || string.IsNullOrEmpty(item.ShoppingCartJson))
            {
                PaymentLogger.Error("BuyNow shopping cart missing after successful payment.");
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }

            BuyNowModel buyNowModel = JsonConvert.DeserializeObject<BuyNowModel>(item.ShoppingCartJson);
            PaymentLogger.Debug("Deserialized BuyNow model from shopping cart.");
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

            if (!string.IsNullOrEmpty(buyNowModel.ConversationId)
                && !string.Equals(paymentResult.ConversationId, buyNowModel.ConversationId, StringComparison.OrdinalIgnoreCase))
            {
                PaymentLogger.Error("BuyNow ConversationId does not match payment ConversationId.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!PaidPriceMatches(paymentResult.PaidPrice, buyNowModel.TotalPriceWithCargoPrice))
            {
                PaymentLogger.Error("BuyNow PaidPrice mismatch.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            buyNowModel.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            buyNowModel.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            buyNowModel.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            buyNowModel.Customer.Lang = CurrentLanguage;
            PaymentLogger.Debug("Updated BuyNow model with cargo and language details.");

            var order = await ShoppingCartService.SaveBuyNowAsync(buyNowModel, paymentResult);
            PaymentLogger.Info($"Order saved with ID: {order.Id}. Cleared BuyNow cart. Redirecting to ThankYouForYourOrder.");
            await ClearBuyNowAsync(buyNowModel);
            PaymentLogger.Debug("Cleared BuyNow cart. Redirecting to ThankYouForYourOrder.");
            TempData[LastCompletedOrderIdKey] = order.Id;
            return RedirectToAction(ThankYouForYourOrderAction, new { orderId = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyCoupon(String couponCode)
        {
            var couponObj = await CouponService.GetCouponByCodeAsync(couponCode, CurrentLanguage);
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            if (couponObj != null)
            {
                shoppingCart.Coupon = Mapper.Map<CouponDto>(couponObj);
            }
            else
            {
                shoppingCart.Coupon = null;
            }
            await SaveShoppingCartAsync(shoppingCart);
            return RedirectToAction(ShoppingCartAction);
        }

        private async Task ClearBuyNowAsync(BuyNowModel buyNowModel)
        {
            PaymentLogger.Info($"Entering ClearBuyNowAsync with OrderGuid: {buyNowModel.OrderGuid}");
            await ShoppingCartService.DeleteByOrderGuidAsync(buyNowModel.OrderGuid);
            PaymentLogger.Info("BuyNow cart deleted from data source.");
        }

        protected void InformCustomerToFillOutForm(CustomerDto customer)
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

            if (customer.IdentityNumber.ToStr().Length != 11)
            {
                ModelState.AddModelError("customer.IdentityNumber", Resource.MandatoryField);
            }
             
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
            PaymentLogger.Info("Completed InformCustomerToFillOutForm validation.");
        }

        protected AddressDto SetAddress(CustomerDto customer, AddressDto address)
        {
            PaymentLogger.Info("Entering SetAddress method.");
            if (address == null)
            {
                PaymentLogger.Info("Address is null. Creating new address.");
                address = new AddressDto();
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

        protected async Task SendNotificationEmailsToCustomerAndAdminUsersForNewOrderAsync(Order order)
        {
            PaymentLogger.Info($"Entering SendEmails for order ID: {order.Id}");

            var emailAccount = await SettingService.GetEmailAccountAsync();
            var customerTemplate = await TryRenderOrderConfirmationEmailAsync(order.Id);
            var adminTemplate = await TryRenderCompanyGotNewOrderEmailAsync(order.Id);

            if (customerTemplate == null && adminTemplate == null)
            {
                PaymentLogger.Error($"No order notification email could be rendered. order.Id:{order.Id}");
                return;
            }

            if (customerTemplate != null)
            {
                try
                {
                    EmailSender.SendRenderedEmailTemplateToCustomer(emailAccount, customerTemplate, sendInBackground: true);
                    PaymentLogger.Info($"Order confirmation email queued for customer. order.Id:{order.Id}");
                }
                catch (Exception e)
                {
                    PaymentLogger.Error(e, $"Failed to queue order confirmation email. order.Id:{order.Id}: {e.Message}");
                }
            }

            if (adminTemplate != null)
            {
                try
                {
                    EmailSender.SendRenderedEmailTemplateToAdminUsers(emailAccount, adminTemplate, sendInBackground: true);
                    PaymentLogger.Info($"New order email queued for admin users. order.Id:{order.Id}");
                }
                catch (Exception e)
                {
                    PaymentLogger.Error(e, $"Failed to queue company new order email. order.Id:{order.Id}: {e.Message}");
                }
            }
        }

        private async Task<Tuple<string, RazorRenderResult, Customer>> TryRenderOrderConfirmationEmailAsync(int orderId)
        {
            try
            {
                var emailTemplate = await RazorEngineHelper.OrderConfirmationEmailAsync(orderId);
                if (emailTemplate?.Item2?.Result == null)
                {
                    PaymentLogger.Error("RazorEngineHelper OrderConfirmationEmail template Is NULL.order.Id:" + orderId);
                    return null;
                }
                PaymentLogger.Info("Generated order confirmation email template.");
                return emailTemplate;
            }
            catch (Exception e)
            {
                PaymentLogger.Error(e, $"Failed to render order confirmation email: {e.Message}");
                return null;
            }
        }

        private async Task<Tuple<string, RazorRenderResult, Customer>> TryRenderCompanyGotNewOrderEmailAsync(int orderId)
        {
            try
            {
                var emailTemplate = await RazorEngineHelper.CompanyGotNewOrderEmailAsync(orderId);
                if (emailTemplate?.Item2?.Result == null)
                {
                    PaymentLogger.Error("RazorEngineHelper CompanyGotNewOrderEmail template Is NULL.order.Id:" + orderId);
                    return null;
                }
                PaymentLogger.Info("Generated company new order email template.");
                return emailTemplate;
            }
            catch (Exception e)
            {
                PaymentLogger.Error(e, $"Failed to render company new order email: {e.Message}");
                return null;
            }
        }

        public async Task<ActionResult> ShoppingWithoutAccount()
        {
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            ViewBag.ShoppingCartSession = shoppingCart;
            var buyWithNoAccountCreation = new BuyWithNoAccountCreation();
            buyWithNoAccountCreation.ShoppingCartItems = shoppingCart.ShoppingCartItems;
            buyWithNoAccountCreation.Coupon = shoppingCart.Coupon;
            buyWithNoAccountCreation.Customer = shoppingCart.Customer;
            PaymentLogger.Info("Returning ShoppingWithoutAccount view.");
            return View(buyWithNoAccountCreation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit("checkout", DefaultLimit = 5, DefaultWindowMinutes = 5)]
        public async Task<ActionResult> ShoppingWithoutAccount(CustomerDto customer)
        {
            PaymentLogger.Info("Entering ContinueShoppingWithoutAccount POST action.");
            if (customer == null)
            {
                PaymentLogger.Error("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
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
                var customerEntity = await CustomerService.SaveOrEditEntityAsync(customer.ToEntity());
                PaymentLogger.Info("Saving customer information,customer.Id:"+ customerEntity.Id);

                shoppingCart.Customer = Mapper.Map<CustomerDto>(customerEntity);
                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                PaymentLogger.Info("Set shipping and billing addresses.");

                ShoppingCart item = await SaveShoppingCartAsync(shoppingCart);
          

                await RevalidateCouponAsync(shoppingCart);
                await SaveShoppingCartAsync(shoppingCart);
                ViewBag.CheckoutFormInitialize = await PaymentContext.InitializeCheckoutAsync(shoppingCart, item.UserId, "ShoppingWithoutAccountResult");
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

        public async Task<ActionResult> ShoppingWithoutAccountResult(PaymentCallbackRequest model, String o, String orderNumber)
        {
            PaymentResultDto paymentResult = await PaymentContext.RetrievePaymentResultAsync(model != null ? model.Token : null);
            PaymentLogger.Info("Entering ShoppingWithoutAccountResult action. Status: {0} ConversationId: {1}", paymentResult.PaymentStatus, paymentResult.ConversationId);

            if (!IsSuccessfulPayment(paymentResult))
            {
                PaymentLogger.Error($"BuyWithNoAccountCreation payment failed. Status: {paymentResult?.PaymentStatus}");
                return RedirectToAction("NoSuccessForYourOrder");
            }

            string orderGuid;
            try
            {
                orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
            }
            catch (Exception ex)
            {
                PaymentLogger.Error(ex, "Failed to decrypt ShoppingWithoutAccount payment callback order reference.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var bindingError = ValidatePaymentBinding(paymentResult, orderGuid, orderNumber);
            if (bindingError != null)
            {
                return bindingError;
            }

            var existingOrder = await FindExistingPaidOrderAsync(paymentResult.PaymentId, orderGuid);
            if (existingOrder != null)
            {
                TempData[LastCompletedOrderIdKey] = existingOrder.Id;
                return RedirectToAction(ThankYouForYourOrderAction, new { orderId = existingOrder.Id });
            }

            var item = await ShoppingCartService.GetShoppingCartByOrderGuidAsync(orderGuid);
            ShoppingCartSession shoppingCart = await GetShoppingCartByOrderGuidAsync(orderGuid);
            if (item == null || string.IsNullOrEmpty(item.ShoppingCartJson) || shoppingCart == null)
            {
                PaymentLogger.Error("ShoppingWithoutAccount cart missing after successful payment.");
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }

            BuyWithNoAccountCreation buyWithNoAccountCreation = JsonConvert.DeserializeObject<BuyWithNoAccountCreation>(item.ShoppingCartJson);
            PaymentLogger.Debug("Deserialized BuyWithNoAccountCreation model from shopping cart.");
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

            if (!PaidPriceMatches(paymentResult.PaidPrice, buyWithNoAccountCreation.TotalPriceWithCargoPrice))
            {
                PaymentLogger.Error("ShoppingWithoutAccount PaidPrice mismatch.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            buyWithNoAccountCreation.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            buyWithNoAccountCreation.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            buyWithNoAccountCreation.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            buyWithNoAccountCreation.Customer.Lang = CurrentLanguage;
            PaymentLogger.Debug("Updated buyWithNoAccountCreation model with cargo and language details.");

            var resolvedOrderNumber = string.IsNullOrWhiteSpace(paymentResult.ConversationId)
                ? orderNumber
                : paymentResult.ConversationId;
            var order = await ShoppingCartService.SaveBuyWithNoAccountCreationAsync(resolvedOrderNumber, buyWithNoAccountCreation, paymentResult);
            PaymentLogger.Info($"Order saved with ID: {order.Id}. Cleared cart. Redirecting to ThankYouForYourOrder.");
            await SendNotificationEmailsToCustomerAndAdminUsersForNewOrderAsync(await OrderService.GetOrderByIdAsync(order.Id));
            await ClearBuyWithNoAccountCreationAsync(buyWithNoAccountCreation);
            await ClearCartAsync(shoppingCart);
            PaymentLogger.Debug("Cleared buyWithNoAccountCreation cart. Redirecting to ThankYouForYourOrder.");
            TempData[LastCompletedOrderIdKey] = order.Id;
            return RedirectToAction(ThankYouForYourOrderAction, new { orderId = order.Id });
        }

        private async Task ClearBuyWithNoAccountCreationAsync(BuyWithNoAccountCreation buyWithNoAccountCreation)
        {
            PaymentLogger.Info($"Entering ClearBuyWithNoAccountCreationAsync with OrderGuid: {buyWithNoAccountCreation.OrderGuid}");
            await ShoppingCartService.DeleteByOrderGuidAsync(buyWithNoAccountCreation.OrderGuid);
            PaymentLogger.Info("BuyNow cart deleted from data source.");
        }

        private async Task RevalidateCouponAsync(ShoppingCartSession shoppingCart)
        {
            if (shoppingCart?.Coupon == null || string.IsNullOrWhiteSpace(shoppingCart.Coupon.Code))
            {
                if (shoppingCart != null)
                {
                    shoppingCart.Coupon = null;
                }
                return;
            }

            try
            {
                var couponEntity = await CouponService.GetCouponByCodeAsync(shoppingCart.Coupon.Code, CurrentLanguage);
                shoppingCart.Coupon = Mapper.Map<CouponDto>(couponEntity);
            }
            catch (Exception ex)
            {
                PaymentLogger.Warn(ex, "Coupon revalidation failed; clearing coupon from cart.");
                shoppingCart.Coupon = null;
            }
        }

        private static bool IsSuccessfulPayment(PaymentResultDto paymentResult)
        {
            return paymentResult != null
                && !string.IsNullOrEmpty(paymentResult.PaymentStatus)
                && paymentResult.PaymentStatus.Equals(Domain.Constants.SUCCESS, StringComparison.InvariantCultureIgnoreCase);
        }

        private static ActionResult ValidatePaymentBinding(PaymentResultDto paymentResult, string orderGuid, string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderGuid))
            {
                PaymentLogger.Error("Payment callback orderGuid is empty.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!string.Equals(paymentResult.BasketId, orderGuid, StringComparison.OrdinalIgnoreCase))
            {
                PaymentLogger.Error("Payment BasketId does not match callback order reference.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(orderNumber)
                && !string.IsNullOrWhiteSpace(paymentResult.ConversationId)
                && !string.Equals(paymentResult.ConversationId, orderNumber, StringComparison.OrdinalIgnoreCase))
            {
                PaymentLogger.Error("Payment ConversationId does not match callback orderNumber.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return null;
        }

        private async Task<Order> FindExistingPaidOrderAsync(string paymentId, string orderGuid)
        {
            var byPaymentId = await OrderService.GetByPaymentIdAsync(paymentId);
            if (byPaymentId != null)
            {
                PaymentLogger.Info($"Idempotent payment callback: existing order {byPaymentId.Id} for PaymentId.");
                return byPaymentId;
            }

            if (!string.IsNullOrWhiteSpace(orderGuid))
            {
                var byOrderGuid = await OrderService.GetByOrderGuidAsync(orderGuid);
                if (byOrderGuid != null
                    && !string.IsNullOrEmpty(byOrderGuid.PaymentId)
                    && string.Equals(byOrderGuid.PaymentStatus, Domain.Constants.SUCCESS, StringComparison.OrdinalIgnoreCase))
                {
                    PaymentLogger.Info($"Idempotent payment callback: existing order {byOrderGuid.Id} for OrderGuid.");
                    return byOrderGuid;
                }
            }

            return null;
        }

        private static bool PaidPriceMatches(string paidPrice, decimal expectedTotal)
        {
            if (string.IsNullOrWhiteSpace(paidPrice))
            {
                return false;
            }

            var normalized = paidPrice.Trim().Replace(",", ".");
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var paid))
            {
                return false;
            }

            // Allow small rounding differences between cart math and Iyzico formatting.
            return Math.Abs(paid - expectedTotal) <= 0.05m;
        }
    }
}
