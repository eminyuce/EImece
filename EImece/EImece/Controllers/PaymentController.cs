using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Models.Payment;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.Services.Payment;
using EImece.Web.Controllers;
using EImece.Web.Filters;
using EImece.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PaymentResultDto = EImece.Domain.Models.Payment.PaymentResult;
using ProductSpecItem = EImece.Domain.Models.FrontModels.ProductSpecItem;

namespace EImece.Controllers
{
    [NoCache]
    public class PaymentController : BaseController
    {
        private static Microsoft.Extensions.Logging.ILogger StaticPaymentLogger =>
            EImece.Domain.Observability.Logging.LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(PaymentController))
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        private const string ShoppingCartAction = "shoppingcart";
        private const string LastCompletedOrderIdKey = "LastCompletedOrderId";
        private const string ThankYouForYourOrderAction = "ThankYouForYourOrder";

        private readonly IMailTemplateService MailTemplateService;
        private readonly IEmailSender EmailSender;
        private readonly ICouponService CouponService;
        private readonly ICouponValidationService CouponValidationService;
        private readonly IRazorEngineHelper RazorEngineHelper;
        private readonly IOrderService OrderService;
        private readonly IAddressService AddressService;
        private readonly ICustomerService CustomerService;
        private readonly PaymentContext PaymentContext;
        private readonly IShoppingCartService ShoppingCartService;
        private readonly IAuthenticationManager AuthenticationManager;
        private readonly IProductService ProductService;
        private readonly ApplicationSignInManager SignInManager;
        private readonly ApplicationUserManager UserManager;

        public PaymentController(ISettingService settingService,
            AutoMapper.IMapper mapper,
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IMailTemplateService mailTemplateService,
            IEmailSender emailSender,
            ICouponService couponService,
            ICouponValidationService couponValidationService,
            IRazorEngineHelper razorEngineHelper,
            IOrderService orderService,
            IAddressService addressService,
            ICustomerService customerService,
            PaymentContext paymentContext,
            IShoppingCartService shoppingCartService,
            IAuthenticationManager authenticationManager,
            IProductService productService, ILogger<PaymentController> logger)
            : base(settingService, mapper, logger)
        {
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            SignInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            MailTemplateService = mailTemplateService ?? throw new ArgumentNullException(nameof(mailTemplateService));
            EmailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            CouponService = couponService ?? throw new ArgumentNullException(nameof(couponService));
            CouponValidationService = couponValidationService ?? throw new ArgumentNullException(nameof(couponValidationService));
            RazorEngineHelper = razorEngineHelper ?? throw new ArgumentNullException(nameof(razorEngineHelper));
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            AddressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
            CustomerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            PaymentContext = paymentContext ?? throw new ArgumentNullException(nameof(paymentContext));
            ShoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            AuthenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
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
                Logger.LogDebug($"Setting UrlReferrer to: {urlReferrer}");
                shoppingCart.UrlReferrer = urlReferrer.ToStr();
            }
            Logger.LogDebug("Returning ShoppingCart view.");
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
                Logger.LogError("Quantity cannot be less than 0 or greater than 1000.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(productId))
            {
                Logger.LogError("Product ID cannot be null or empty.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(orderGuid))
            {
                Logger.LogError("OrderGuid cannot be null or empty.");
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
            Logger.LogDebug($"Entering AddToCart action with productId: {productId}, quantity: {quantity}, orderGuid: {orderGuid}");
            int pId = GeneralHelper.RevertId(productId);
            Logger.LogDebug($"Reverted productId to: {pId}");
            var product = await ProductService.GetStorefrontProductCardByIdAsync(pId);
            if (product != null)
            {
                Logger.LogDebug($"Product found with ID: {pId}");
                var shoppingCart = await GetShoppingCartAsync();
                if (string.IsNullOrEmpty(shoppingCart.OrderGuid))
                {
                    shoppingCart.OrderGuid = orderGuid;
                }
                else if (!shoppingCart.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"OrderGuid does not match. Setting new OrderGuid: {orderGuid}");
                }

                Logger.LogDebug($"Set shopping cart OrderGuid to: {orderGuid}");

                var item = new ShoppingCartItem();
                var selectedTotalSpecs = new List<ProductSpecItem>();
                if (!string.IsNullOrEmpty(productSpecItems))
                {
                    Logger.LogDebug("Deserializing productSpecItems.");
                    var ooo = JsonConvert.DeserializeObject<ProductSpecItemRoot>(productSpecItems);
                    selectedTotalSpecs = ooo.selectedTotalSpecs;
                    Logger.LogDebug($"Found {selectedTotalSpecs.Count} product specifications.");
                }
                item.Product = new ShoppingCartProduct(product, selectedTotalSpecs);
                item.Quantity = quantity;
                item.ShoppingCartItemId = Guid.NewGuid().ToString();
                Logger.LogDebug($"Created shopping cart item with ID: {item.ShoppingCartItemId}");
                shoppingCart.Add(item);
                Logger.LogDebug("Added item to shopping cart.");
                // Revalidate coupon after cart change per spec 13
                await RevalidateCouponAsync(shoppingCart);
                await SaveShoppingCartAsync(shoppingCart);
                Logger.LogDebug("Returning success JSON response.");
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Logger.LogError($"Product not found with ID: {pId}");
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
            Logger.LogDebug("Returning JSON response with HTML.");
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
            Logger.LogDebug("Returning JSON response with HTML.");
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
            Logger.LogDebug("Rendering _ShoppingCartLinks partial view.");
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
            Logger.LogDebug($"Saving shopping cart with OrderGuid: {item.OrderGuid}, UserId: {item.UserId}");

            shoppingCart.CurrentLanguage = CurrentLanguage;
            await ShoppingCartService.SaveOrEditShoppingCartAsync(item);
            Logger.LogDebug("Shopping cart saved to data source.");

            return item;
        }

        private async Task<string> getUserIdAsync()
        {
            if (Request.IsAuthenticated)
            {
                Logger.LogDebug("Request is authenticated.");
                var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                if (user != null)
                {
                    Logger.LogDebug($"User found with ID: {user.Id}");
                    return user.Id;
                }
                Logger.LogDebug("No user found.");
            }
            return string.Empty;
        }

        private async Task<ShoppingCartSession> GetShoppingCartFromDataSourceAsync()
        {
            Logger.LogDebug("Entering GetShoppingCartFromDataSourceAsync method.");
            HttpCookie orderGuid = Request.Cookies[Domain.Constants.OrderGuidCookieKey];
            string orderGuid2 = orderGuid == null ? null : orderGuid.Value;
            Logger.LogDebug($"Retrieved OrderGuid from cookie: {orderGuid2}");
            var result = await GetShoppingCartByOrderGuidAsync(orderGuid2);
            Logger.LogDebug("Shopping cart retrieved from GetShoppingCartByOrderGuidAsync.");
            return result;
        }

        private async Task<ShoppingCartSession> GetShoppingCartByOrderGuidAsync(string orderGuid)
        {
            ShoppingCartSession result = null;
            var item = orderGuid != null ? await ShoppingCartService.GetShoppingCartByOrderGuidAsync(orderGuid) : null;
            if (item == null)
            {
                Logger.LogDebug("No existing shopping cart found. Creating default shopping cart.");
                result = ShoppingCartSession.CreateDefaultShopingCard(CurrentLanguage, GeneralHelper.GetIpAddress());
                await GetCustomerIfAuthenticatedAsync(result);
            }
            else
            {
                Logger.LogDebug("Existing shopping cart found. Deserializing JSON.");
                result = JsonConvert.DeserializeObject<ShoppingCartSession>(item.ShoppingCartJson);
                string userId = result.Customer != null ? result.Customer.UserId : "";
                item.UserId = string.IsNullOrEmpty(userId) ? await getUserIdAsync() : userId;
                Logger.LogDebug($"Updated shopping cart UserId to: {item.UserId}");
            }

            result.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            result.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            result.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            Logger.LogDebug("Set cargo details for shopping cart.");
            return result;
        }

        private async Task GetCustomerIfAuthenticatedAsync(ShoppingCartSession result)
        {
            if (!Request.IsAuthenticated)
            {
                Logger.LogDebug("Request is not authenticated. No customer assigned.");
                return;
            }

            var userName = User.Identity.GetUserName();
            if (string.IsNullOrWhiteSpace(userName))
            {
                Logger.LogDebug("Authenticated identity has no user name. No customer assigned.");
                return;
            }

            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                Logger.LogWarning("No AspNet user for authenticated name '{0}'. Skipping customer assignment.", userName);
                return;
            }

            Logger.LogDebug($"User found with ID: {user.Id}");
            var c = await CustomerService.GetUserIdAsync(user.Id);
            if (c == null)
            {
                Logger.LogDebug("No customer found. Creating new customer.");
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
            Logger.LogDebug("Entering GetShoppingCartAsync method.");
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            Logger.LogDebug("Shopping cart retrieved.");
            return shoppingCart;
        }

        public async Task<ActionResult> CheckoutBillingDetails()
        {
            if (Request.IsAuthenticated)
            {
                ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
                if (shoppingCart.ShoppingCartItems.IsNotEmpty())
                {
                    Logger.LogDebug("Shopping cart has items.");
                    if (shoppingCart.Customer == null)
                    {
                        Logger.LogDebug("No customer in shopping cart. Creating new customer.");
                        shoppingCart.Customer = new CustomerDto();
                        shoppingCart.Customer.CustomerType = (int)EImeceCustomerType.Normal;
                        shoppingCart.Customer.Country = Domain.Constants.IYZICO_ADDRESS_COUNTRY;
                        shoppingCart.Customer.Ip = GeneralHelper.GetIpAddress();
                    }
                    if (shoppingCart.Customer.IsEmpty())
                    {
                        Logger.LogDebug("Customer is empty. Populating from authenticated user.");
                        await GetCustomerIfAuthenticatedAsync(shoppingCart);
                    }
                    Logger.LogDebug("Returning CheckoutBillingDetails view.");
                    return View(shoppingCart);
                }
                else
                {
                    Logger.LogDebug("Shopping cart is empty. Redirecting to shoppingcart.");
                    TempData["StatusMessage"] = "Sepetiniz boÅŸ";
                    return RedirectToAction(ShoppingCartAction, Domain.Constants.PaymentAction);
                }
            }
            else
            {
                // Membership checkout requires an account + address; send guests to register
                // (not login) so they can create a profile before billing details.
                Logger.LogDebug("User is not authenticated. Redirecting to register for membership checkout.");
                return RedirectToAction("Register", "Account",
                    new { returnUrl = Url.Action("CheckoutBillingDetails", Domain.Constants.PaymentAction) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckoutBillingDetails(CustomerDto customer)
        {
            Logger.LogDebug("Entering CheckoutBillingDetails POST action.");
            if (customer == null)
            {
                Logger.LogError("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }
            bool isValidCustomer = customer.isValidCustomer();
            Logger.LogDebug($"Customer validation result: {isValidCustomer}");
            if (isValidCustomer)
            {
                ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
                customer.CustomerType = (int)EImeceCustomerType.Normal;
                shoppingCart.Customer = customer;
                var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                shoppingCart.Customer.UserId = user.Id;
                Logger.LogDebug($"Assigned UserId: {user.Id} to customer.");
                if (customer.IsSameAsShippingAddress)
                {
                    Logger.LogDebug("Shipping address is same as billing address.");
                }

                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                Logger.LogDebug("Set shipping and billing addresses.");

                await SaveShoppingCartAsync(shoppingCart);
                Logger.LogDebug("Shopping cart saved with billing details.");
                Logger.LogDebug("Redirecting to CheckoutPaymentOrderReview.");
                return RedirectToAction("CheckoutPaymentOrderReview");
            }
            else
            {
                Logger.LogInformation("Customer validation failed. Informing customer.");
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

            var orderDto = await OrderService.GetStorefrontOrderConfirmationByOrderNumberAsync(orderNumber);
            if (orderDto == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
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
                Logger.LogDebug("Shopping cart has items. Returning view.");
                return View(shoppingCart);
            }
            else
            {
                Logger.LogDebug("Shopping cart is empty. Redirecting to shoppingcart.");
                return RedirectToAction(ShoppingCartAction, Domain.Constants.PaymentAction);
            }
        }

        public async Task<JsonResult> renderShoppingCartPrice()
        {
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            String cargoPriceHtml = "";
            if (shoppingCart.CargoPriceValue == 0)
            {
                Logger.LogDebug("Cargo price is 0. Setting free shipping HTML.");
                cargoPriceHtml = string.Format("<span class='badge badge-pill badge-danger mr-2 mb-2'>{0}</span>", Resource.CargoFreeTextInfo);
            }
            else
            {
                Logger.LogDebug($"Cargo price is {shoppingCart.CargoPriceValue}. Formatting HTML.");
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
            Logger.LogDebug($"Entering sendOrderComments with orderComments: {orderComments}, orderGuid: {orderGuid}");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            shoppingCart.OrderComments = orderComments;
            Logger.LogDebug("Order comments assigned to shopping cart.");
            await SaveShoppingCartAsync(shoppingCart);
            Logger.LogDebug("Returning success JSON response.");
            return Json(new { status = Domain.Constants.SUCCESS }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> UpdateQuantity(String shoppingItemId, int quantity)
        {
            Logger.LogDebug($"Entering UpdateQuantity with shoppingItemId: {shoppingItemId}, quantity: {quantity}");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                Logger.LogDebug($"Found item with ID: {shoppingItemId}. Updating quantity to: {quantity}");
                item.Quantity = quantity;
                await RevalidateCouponAsync(shoppingCart);
                await SaveShoppingCartAsync(shoppingCart);
                Logger.LogDebug("Shopping cart saved with updated quantity.");
                Logger.LogDebug("Returning success JSON response.");
                var lineTotal = item.TotalPrice.CurrencySign();
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId, LineTotal = lineTotal }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Logger.LogError($"Item with ID: {shoppingItemId} not found.");
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> RemoveCart(String shoppingItemId)
        {
            Logger.LogDebug($"Entering RemoveCart with shoppingItemId: {shoppingItemId}");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            var item = shoppingCart.ShoppingCartItems.FirstOrDefault(r => r.ShoppingCartItemId.Equals(shoppingItemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                Logger.LogDebug($"Found item with ID: {shoppingItemId}. Removing from cart.");
                shoppingCart.ShoppingCartItems.Remove(item);
                if (shoppingCart.ShoppingCartItems.IsEmpty())
                {
                    Logger.LogDebug("Shopping cart is now empty. Clearing coupon.");
                    shoppingCart.ClearValidatedCoupon();
                }
                else
                {
                    await RevalidateCouponAsync(shoppingCart);
                }
                await SaveShoppingCartAsync(shoppingCart);
                return Json(new { status = Domain.Constants.SUCCESS, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Logger.LogError($"Item with ID: {shoppingItemId} not found.");
                Logger.LogDebug("Returning failed JSON response.");
                return Json(new { status = Domain.Constants.FAILED, shoppingItemId, TotalItemCount = shoppingCart.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
        }

        [RateLimit("checkout", DefaultLimit = 5, DefaultWindowMinutes = 5)]
        public async Task<ActionResult> PlaceOrder()
        {
            Logger.LogDebug("Entering PlaceOrder action.");
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();

            if (shoppingCart == null || shoppingCart.ShoppingCartItems.IsEmpty())
            {
                Logger.LogDebug("Shopping cart is null or empty. Redirecting to shoppingcart.");
                return RedirectToAction(ShoppingCartAction, Domain.Constants.PaymentAction);
            }
            if (shoppingCart.Customer.isValidCustomer() && shoppingCart.ShoppingCartItems.IsNotEmpty())
            {
                Logger.LogDebug("Customer is valid and cart has items.");
                if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    await RevalidateCouponAsync(shoppingCart);
                    await SaveShoppingCartAsync(shoppingCart);
                    var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                    Logger.LogInformation($"Initializing checkout form for user ID: {user.Id} via {PaymentContext.ProviderName}");
                    try
                    {
                        if (!AppConfig.HasConfiguredIyzicoCredentials)
                        {
                            Logger.LogWarning("Iyzico API keys are empty; skipping checkout form initialize.");
                            ViewBag.CheckoutFormInitialize = new PaymentInitializeResult
                            {
                                ErrorMessage = Resource.PaymentFormCouldNotBeInitializedConfig,
                                ProviderName = PaymentContext.ProviderName
                            };
                            ModelState.AddModelError("", Resource.PaymentFormCouldNotBeInitializedConfig);
                        }
                        else
                        {
                            var checkoutInit = await PaymentContext.InitializeCheckoutAsync(shoppingCart, user.Id);
                            ViewBag.CheckoutFormInitialize = checkoutInit;
                            if (checkoutInit != null
                                && !string.Equals(checkoutInit.Status, "success", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.LogError(
                                    "PlaceOrder Iyzico initialize rejected status={0} errorCode={1} errorMessage={2} conversationId={3}",
                                    checkoutInit.Status,
                                    checkoutInit.ErrorCode,
                                    checkoutInit.ErrorMessage,
                                    checkoutInit.ConversationId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to initialize payment checkout form via {0}.", PaymentContext.ProviderName);
                        ViewBag.CheckoutFormInitialize = null;
                        ModelState.AddModelError("", Resource.PaymentFormCouldNotBeInitializedSettings);
                    }
                    Logger.LogDebug("Returning PlaceOrder view.");
                    return View(shoppingCart);
                }
            }
            else
            {
                // Guest contact details missing: send the customer to the billing details
                // step (which shows the membership/registration gate) instead of raw text.
                Logger.LogInformation("PlaceOrder called without customer details. Redirecting to CheckoutBillingDetails.");
                return RedirectToAction("CheckoutBillingDetails", Domain.Constants.PaymentAction);
            }
        }

        public async Task<ActionResult> PaymentResult(PaymentCallbackRequest model, string o, string u, String orderNumber)
        {
            // iyzico Checkout Form callback POSTs "token" â€” same binding as the former RetrieveCheckoutFormRequest.
            PaymentResultDto paymentResult = await PaymentContext.RetrievePaymentResultAsync(model != null ? model.Token : null);
            Logger.LogInformation($"PaymentResult with ACCOUNT status: {paymentResult.PaymentStatus} ConversationId: {paymentResult.ConversationId}");
            if (!IsSuccessfulPayment(paymentResult))
            {
                Logger.LogError($"Payment failed. Status: {paymentResult?.PaymentStatus}");
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
                Logger.LogError(ex, "Failed to decrypt payment callback references.");
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
                Logger.LogError($"Shopping cart missing for OrderGuid after successful payment.");
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }

            if (!PaidPriceMatches(paymentResult.PaidPrice, shoppingCart.TotalPriceWithCargoPrice))
            {
                Logger.LogError($"PaidPrice mismatch for OrderGuid. Expected cart total does not match payment.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var resolvedOrderNumber = string.IsNullOrWhiteSpace(paymentResult.ConversationId)
                ? orderNumber
                : paymentResult.ConversationId;
            var order = await ShoppingCartService.SaveShoppingCartAsync(resolvedOrderNumber, shoppingCart, paymentResult, userId);
            Logger.LogDebug($"Order saved with ID: {order.Id}. Cart cleared. Redirecting to ThankYouForYourOrder.");
            await SendNotificationEmailsToCustomerAndAdminUsersForNewOrderAsync(order.Id);
            await ClearCartAsync(shoppingCart);
            Logger.LogDebug("Cart cleared. Redirecting to ThankYouForYourOrder.");
            TempData[LastCompletedOrderIdKey] = order.Id;
            return RedirectToAction(ThankYouForYourOrderAction, new { orderId = order.Id });
        }

        public async Task<ActionResult> ThankYouForYourOrder(int orderId)
        {
            string restrictToUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                restrictToUserId = User.Identity.GetUserId();
            }
            else
            {
                var lastCompletedOrderId = TempData[LastCompletedOrderIdKey] as int?;
                if (!lastCompletedOrderId.HasValue || lastCompletedOrderId.Value != orderId)
                {
                    return new HttpUnauthorizedResult();
                }
            }

            var orderDto = await OrderService.GetStorefrontOrderConfirmationByIdAsync(orderId, restrictToUserId);
            if (orderDto == null)
            {
                return HttpNotFound();
            }

            return View(orderDto);
        }

        public ActionResult NoSuccessForYourOrder()
        {
            return View();
        }

        private async Task ClearCartAsync(ShoppingCartSession shoppingCart)
        {
            Logger.LogDebug("Entering ClearCartAsync method.");
            if (Request.Browser.Cookies)
            {
                Logger.LogDebug("Removing OrderGuid cookie.");
                Response.Cookies.Remove(Domain.Constants.OrderGuidCookieKey);
                var aCookie = new HttpCookie(Domain.Constants.OrderGuidCookieKey)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection
                };
                Response.Cookies.Add(aCookie);
                Logger.LogDebug("Added expired cookie to response.");
            }
            await ShoppingCartService.DeleteByOrderGuidAsync(shoppingCart.OrderGuid);
            Logger.LogDebug($"Deleted shopping cart with OrderGuid: {shoppingCart.OrderGuid}");
        }

        public async Task<ActionResult> BuyNow(String id)
        {
            Logger.LogDebug($"Entering BuyNow with id: {id}");
            if (String.IsNullOrEmpty(id))
            {
                Logger.LogError("Product ID is null or empty.");
                Logger.LogDebug("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var productId = id.GetId();
                Logger.LogDebug($"Converted product ID to: {productId}");
                BuyNowModel buyNowModel = await CreateBuyNowModelAsync(productId);
                Logger.LogDebug("Created BuyNow model.");
                ViewBag.SeoId = buyNowModel.ProductDetailViewModel.ProductDto.SeoUrl;
                Logger.LogDebug($"Set SeoId in ViewBag: {ViewBag.SeoId}");
                Logger.LogDebug("Returning BuyNow view.");
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
            Logger.LogDebug($"Entering BuyNow POST with productId: {productId}");
            if (customer == null)
            {
                Logger.LogError("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }

            bool isValidCustomer = customer.isValidCustomer();
            Logger.LogDebug($"Customer validation result: {isValidCustomer}");
            BuyNowModel buyNowModel = await CreateBuyNowModelAsync(GeneralHelper.RevertId(productId));
            buyNowModel.Customer = customer;
            Logger.LogDebug("Assigned customer to BuyNow model.");

            if (isValidCustomer)
            {
                customer.CustomerType = (int)EImeceCustomerType.BuyNow;
                buyNowModel.ShippingAddress = SetAddress(customer, buyNowModel.ShippingAddress);
                buyNowModel.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                buyNowModel.OrderGuid = Guid.NewGuid().ToString();
                Logger.LogDebug($"Set shipping address and OrderGuid: {buyNowModel.OrderGuid}");

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
                Logger.LogDebug("Saved BuyNow shopping cart.");

                ViewBag.CheckoutFormInitialize = await PaymentContext.InitializeBuyNowAsync(buyNowModel);
                Logger.LogInformation("Initialized checkout form for BuyNow.");
                Logger.LogDebug("Returning BuyNowPayment view.");
                return View("BuyNowPayment", buyNowModel);
            }
            else
            {
                Logger.LogInformation("Customer validation failed. Informing customer.");
                InformCustomerToFillOutForm(customer);
                Logger.LogDebug("Returning BuyNow view with validation errors.");
                return View(buyNowModel);
            }
        }

        private async Task<BuyNowModel> CreateBuyNowModelAsync(int productId)
        {
            Logger.LogDebug($"Entering CreateBuyNowModelAsync with productId: {productId}");
            BuyNowModel buyNowModel = new BuyNowModel();
            buyNowModel.ProductId = productId;
            buyNowModel.ProductDetailViewModel = await ProductService.GetProductDetailViewModelByIdAsync(productId);
            Logger.LogDebug("Set product details in BuyNow model.");
            buyNowModel.ShoppingCartItem = new ShoppingCartItem();
            buyNowModel.ShoppingCartItem.Product = new ShoppingCartProduct(buyNowModel.ProductDetailViewModel.ProductDto, new List<ProductSpecItem>());
            buyNowModel.ShoppingCartItem.Quantity = 1;
            buyNowModel.ShoppingCartItem.ShoppingCartItemId = Guid.NewGuid().ToString();
            Logger.LogDebug($"Created shopping cart item with ID: {buyNowModel.ShoppingCartItem.ShoppingCartItemId}");
            buyNowModel.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            buyNowModel.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            buyNowModel.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            Logger.LogDebug("Set cargo details in BuyNow model.");
            return buyNowModel;
        }

        public async Task<ActionResult> BuyNowPaymentResult(PaymentCallbackRequest model, String o)
        {
            PaymentResultDto paymentResult = await PaymentContext.RetrievePaymentResultAsync(model != null ? model.Token : null);
            Logger.LogInformation("BuyNowPaymentResult. Payment status: {0}", paymentResult.PaymentStatus);
            if (!IsSuccessfulPayment(paymentResult))
            {
                Logger.LogError($"BuyNow payment failed. Status: {paymentResult?.PaymentStatus}");
                return RedirectToAction("NoSuccessForYourOrder");
            }

            string orderGuid;
            try
            {
                orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to decrypt BuyNow payment callback order reference.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!string.Equals(paymentResult.BasketId, orderGuid, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogError("BuyNow payment BasketId does not match callback order reference.");
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
                Logger.LogError("BuyNow shopping cart missing after successful payment.");
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }

            BuyNowModel buyNowModel = JsonConvert.DeserializeObject<BuyNowModel>(item.ShoppingCartJson);
            Logger.LogDebug("Deserialized BuyNow model from shopping cart.");
            if (buyNowModel.ShoppingCartItem == null || buyNowModel.ShoppingCartItem.Product == null)
            {
                Logger.LogError("ShoppingCartItem or Product is null in BuyNow model.");
                throw new ArgumentException("buyNowModel.ShoppingCartItem.Product cannot be null");
            }
            if (buyNowModel.Customer == null)
            {
                Logger.LogError("Customer is null in BuyNow model.");
                throw new ArgumentException("buyNowModel.Customer cannot be null");
            }

            if (!string.IsNullOrEmpty(buyNowModel.ConversationId)
                && !string.Equals(paymentResult.ConversationId, buyNowModel.ConversationId, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogError("BuyNow ConversationId does not match payment ConversationId.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!PaidPriceMatches(paymentResult.PaidPrice, buyNowModel.TotalPriceWithCargoPrice))
            {
                Logger.LogError("BuyNow PaidPrice mismatch.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            buyNowModel.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            buyNowModel.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            buyNowModel.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            buyNowModel.Customer.Lang = CurrentLanguage;
            Logger.LogDebug("Updated BuyNow model with cargo and language details.");

            var order = await ShoppingCartService.SaveBuyNowAsync(buyNowModel, paymentResult);
            Logger.LogDebug($"Order saved with ID: {order.Id}. Cleared BuyNow cart. Redirecting to ThankYouForYourOrder.");
            await ClearBuyNowAsync(buyNowModel);
            Logger.LogDebug("Cleared BuyNow cart. Redirecting to ThankYouForYourOrder.");
            TempData[LastCompletedOrderIdKey] = order.Id;
            return RedirectToAction(ThankYouForYourOrderAction, new { orderId = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyCoupon(String couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                TempData["CouponMessage"] = "Coupon code required";
                TempData["CouponMessageType"] = "danger";
                return RedirectToAction(ShoppingCartAction);
            }
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null;

            // Prevent stacking: only one coupon per order unless AllowStacking
            if (shoppingCart.Coupon != null && !string.IsNullOrWhiteSpace(shoppingCart.Coupon.Code))
            {
                if (!string.Equals(shoppingCart.Coupon.Code, couponCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Need to check stacking via validation service; for now block if existing not allow stacking
                    TempData["CouponMessage"] = "Only one coupon per order is allowed. Remove existing coupon first.";
                    TempData["CouponMessageType"] = "danger";
                    return RedirectToAction(ShoppingCartAction);
                }
            }

            // Use central validation
            if (CouponValidationService != null)
            {
                try
                {
                    var ctx = await BuildCouponValidationContextAsync(shoppingCart, userId).ConfigureAwait(false);
                    var validation = await CouponValidationService.ValidateCouponAsync(couponCode, shoppingCart, ctx).ConfigureAwait(false);
                    if (!validation.IsValid)
                    {
                        TempData["CouponMessage"] = validation.Message ?? validation.Reason.ToString();
                        TempData["CouponMessageType"] = "danger";
                        // Do not apply coupon
                        shoppingCart.ClearValidatedCoupon();
                        await SaveShoppingCartAsync(shoppingCart);
                        return RedirectToAction(ShoppingCartAction);
                    }
                    // Fetch coupon DTO for storage (includes all fields)
                    CouponDto couponDto = null;
                    try { couponDto = await CouponService.GetStorefrontCouponByCodeAsync(couponCode, CurrentLanguage); } catch { }
                    if (couponDto == null)
                    {
                        // Fallback: minimal DTO from validation
                        couponDto = new CouponDto { Code = couponCode, Name = couponCode };
                    }
                    shoppingCart.Coupon = couponDto;
                    shoppingCart.SetValidatedCouponDiscount(validation.DiscountAmount, validation.ShippingDiscount, validation.EligibleAmount);
                    await SaveShoppingCartAsync(shoppingCart);
                    TempData["CouponMessage"] = validation.Message ?? "Coupon applied successfully.";
                    TempData["CouponMessageType"] = "success";
                    return RedirectToAction(ShoppingCartAction);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ApplyCoupon validation failed");
                    TempData["CouponMessage"] = "Coupon validation failed: " + ex.Message;
                    TempData["CouponMessageType"] = "danger";
                    return RedirectToAction(ShoppingCartAction);
                }
            }
            // Fallback minimal logic if validation service not available
            var fallbackDto = await CouponService.GetStorefrontCouponByCodeAsync(couponCode, CurrentLanguage);
            if (fallbackDto == null)
            {
                TempData["CouponMessage"] = "Coupon not found or expired";
                TempData["CouponMessageType"] = "danger";
                shoppingCart.ClearValidatedCoupon();
                await SaveShoppingCartAsync(shoppingCart);
                return RedirectToAction(ShoppingCartAction);
            }
            shoppingCart.Coupon = fallbackDto;
            await SaveShoppingCartAsync(shoppingCart);
            TempData["CouponMessage"] = "Coupon applied successfully.";
            TempData["CouponMessageType"] = "success";
            return RedirectToAction(ShoppingCartAction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveCoupon()
        {
            var shoppingCart = await GetShoppingCartFromDataSourceAsync();
            shoppingCart.ClearValidatedCoupon();
            await SaveShoppingCartAsync(shoppingCart);
            TempData["CouponMessage"] = "Coupon removed.";
            TempData["CouponMessageType"] = "info";
            return RedirectToAction(ShoppingCartAction);
        }

        private async Task<Domain.Models.CouponValidationContext> BuildCouponValidationContextAsync(ShoppingCartSession shoppingCart, string userId)
        {
            var isAuth = !string.IsNullOrEmpty(userId) && User.Identity.IsAuthenticated;
            var ctx = new Domain.Models.CouponValidationContext
            {
                UserId = userId,
                IsAuthenticated = isAuth,
                Language = CurrentLanguage,
                Currency = null, // Currency check deferred to order creation when payment currency known
                CargoPrice = shoppingCart.CargoPriceValue,
                HasExistingCoupon = shoppingCart.Coupon != null && !string.IsNullOrWhiteSpace(shoppingCart.Coupon.Code),
                ExistingCouponCode = shoppingCart.Coupon?.Code
            };
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var cust = await CustomerService.GetUserIdAsync(userId).ConfigureAwait(false);
                    if (cust != null)
                    {
                        ctx.CustomerId = cust.Id;
                        ctx.BirthDate = cust.BirthDate;
                        ctx.CustomerCreatedDate = cust.CreatedDate;
                    }
                    else
                    {
                        // Try via UserManager lookup for birthdate from customer? fallback
                    }
                }
            }
            catch (Exception ex) { Logger.LogWarning(ex, "Failed to build coupon validation context"); }
            return ctx;
        }

        private async Task ClearBuyNowAsync(BuyNowModel buyNowModel)
        {
            Logger.LogDebug($"Entering ClearBuyNowAsync with OrderGuid: {buyNowModel.OrderGuid}");
            await ShoppingCartService.DeleteByOrderGuidAsync(buyNowModel.OrderGuid);
            Logger.LogDebug("BuyNow cart deleted from data source.");
        }

        protected void InformCustomerToFillOutForm(CustomerDto customer)
        {
            Logger.LogDebug("Entering InformCustomerToFillOutForm method.");
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
            Logger.LogDebug("Completed InformCustomerToFillOutForm validation.");
        }

        protected AddressDto SetAddress(CustomerDto customer, AddressDto address)
        {
            Logger.LogDebug("Entering SetAddress method.");
            if (address == null)
            {
                Logger.LogDebug("Address is null. Creating new address.");
                address = new AddressDto();
            }
            if (customer == null)
            {
                Logger.LogError("Customer is null. Throwing exception.");
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
            Logger.LogDebug("Address set with customer details.");
            return address;
        }

        protected async Task SendNotificationEmailsToCustomerAndAdminUsersForNewOrderAsync(int orderId)
        {
            Logger.LogDebug($"Entering SendEmails for order ID: {orderId}");

            var emailAccount = await SettingService.GetEmailAccountAsync();
            var customerTemplate = await TryRenderOrderConfirmationEmailAsync(orderId);
            var adminTemplate = await TryRenderCompanyGotNewOrderEmailAsync(orderId);

            if (customerTemplate == null && adminTemplate == null)
            {
                Logger.LogError($"No order notification email could be rendered. order.Id:{orderId}");
                return;
            }

            if (customerTemplate != null)
            {
                try
                {
                    EmailSender.SendRenderedEmailTemplateToCustomer(emailAccount, customerTemplate, sendInBackground: true);
                    Logger.LogInformation($"Order confirmation email queued for customer. order.Id:{orderId}");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Failed to queue order confirmation email. order.Id:{orderId}: {e.Message}");
                }
            }

            if (adminTemplate != null)
            {
                try
                {
                    EmailSender.SendRenderedEmailTemplateToAdminUsers(emailAccount, adminTemplate, sendInBackground: true);
                    Logger.LogInformation($"New order email queued for admin users. order.Id:{orderId}");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Failed to queue company new order email. order.Id:{orderId}: {e.Message}");
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
                    Logger.LogError("RazorEngineHelper OrderConfirmationEmail template Is NULL.order.Id:" + orderId);
                    return null;
                }
                Logger.LogDebug("Generated order confirmation email template.");
                return emailTemplate;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to render order confirmation email: {e.Message}");
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
                    Logger.LogError("RazorEngineHelper CompanyGotNewOrderEmail template Is NULL.order.Id:" + orderId);
                    return null;
                }
                Logger.LogDebug("Generated company new order email template.");
                return emailTemplate;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to render company new order email: {e.Message}");
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
            Logger.LogDebug("Returning ShoppingWithoutAccount view.");
            return View(buyWithNoAccountCreation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit("checkout", DefaultLimit = 5, DefaultWindowMinutes = 5)]
        public async Task<ActionResult> ShoppingWithoutAccount(CustomerDto customer)
        {
            Logger.LogDebug("Entering ContinueShoppingWithoutAccount POST action.");
            if (customer == null)
            {
                Logger.LogError("Customer is null. Throwing exception.");
                throw new NotSupportedException();
            }
            ShoppingCartSession shoppingCart = await GetShoppingCartAsync();
            ViewBag.ShoppingCartSession = shoppingCart;

            var buyWithNoAccountCreation = new BuyWithNoAccountCreation();
            buyWithNoAccountCreation.OrderGuid = shoppingCart.OrderGuid;
            buyWithNoAccountCreation.ShoppingCartItems = shoppingCart.ShoppingCartItems;
            buyWithNoAccountCreation.Coupon = shoppingCart.Coupon;
            bool isValidCustomer = customer.isValidCustomer();
            Logger.LogDebug($"Customer validation result: {isValidCustomer}");
            if (isValidCustomer)
            {
                Logger.LogDebug("Saving customer information");

                customer.CustomerType = (int)EImeceCustomerType.ShoppingWithoutAccount;
                customer.Country = Domain.Constants.IYZICO_ADDRESS_COUNTRY;
                customer.Ip = GeneralHelper.GetIpAddress();
                customer.CreatedDate = DateTime.Now;
                customer.UpdatedDate = DateTime.Now;
                customer.GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber);
                customer.UserId = Guid.NewGuid().ToString();
                var customerEntity = await CustomerService.SaveOrEditEntityAsync(customer.ToEntity());
                Logger.LogDebug("Saving customer information,customer.Id:" + customerEntity.Id);

                shoppingCart.Customer = Mapper.Map<CustomerDto>(customerEntity);
                shoppingCart.ShippingAddress = SetAddress(customer, shoppingCart.ShippingAddress);
                shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shoppingCart.BillingAddress = SetAddress(customer, shoppingCart.BillingAddress);
                shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                Logger.LogDebug("Set shipping and billing addresses.");

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
                buyWithNoAccountCreation.Customer = customer;
                Logger.LogDebug("Returning view with validation errors.");
                return View(buyWithNoAccountCreation);
            }
        }

        public async Task<ActionResult> ShoppingWithoutAccountResult(PaymentCallbackRequest model, String o, String orderNumber)
        {
            PaymentResultDto paymentResult = await PaymentContext.RetrievePaymentResultAsync(model != null ? model.Token : null);
            Logger.LogInformation("ShoppingWithoutAccountResult. Status: {0} ConversationId: {1}", paymentResult.PaymentStatus, paymentResult.ConversationId);

            if (!IsSuccessfulPayment(paymentResult))
            {
                Logger.LogError($"BuyWithNoAccountCreation payment failed. Status: {paymentResult?.PaymentStatus}");
                return RedirectToAction("NoSuccessForYourOrder");
            }

            string orderGuid;
            try
            {
                orderGuid = EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to decrypt ShoppingWithoutAccount payment callback order reference.");
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
                Logger.LogError("ShoppingWithoutAccount cart missing after successful payment.");
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }

            BuyWithNoAccountCreation buyWithNoAccountCreation = JsonConvert.DeserializeObject<BuyWithNoAccountCreation>(item.ShoppingCartJson);
            Logger.LogDebug("Deserialized BuyWithNoAccountCreation model from shopping cart.");
            if (buyWithNoAccountCreation.ShoppingCartItems.IsEmpty())
            {
                Logger.LogError("ShoppingCartItem or Product is null in buyWithNoAccountCreation model.");
                throw new ArgumentException("buyWithNoAccountCreation.ShoppingCartItem.ShoppingCartItems cannot be empty");
            }
            if (buyWithNoAccountCreation.Customer == null)
            {
                Logger.LogError("Customer is null in BuyNow model.");
                throw new ArgumentException("buyWithNoAccountCreation.Customer cannot be null");
            }

            if (!PaidPriceMatches(paymentResult.PaidPrice, buyWithNoAccountCreation.TotalPriceWithCargoPrice))
            {
                Logger.LogError("ShoppingWithoutAccount PaidPrice mismatch.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            buyWithNoAccountCreation.CargoCompany = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoCompany);
            buyWithNoAccountCreation.BasketMinTotalPriceForCargo = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.BasketMinTotalPriceForCargo);
            buyWithNoAccountCreation.CargoPrice = await SettingService.GetSettingValueDtoByKeyAsync(Domain.Constants.CargoPrice);
            buyWithNoAccountCreation.Customer.Lang = CurrentLanguage;
            Logger.LogDebug("Updated buyWithNoAccountCreation model with cargo and language details.");

            var resolvedOrderNumber = string.IsNullOrWhiteSpace(paymentResult.ConversationId)
                ? orderNumber
                : paymentResult.ConversationId;
            var order = await ShoppingCartService.SaveBuyWithNoAccountCreationAsync(resolvedOrderNumber, buyWithNoAccountCreation, paymentResult);
            Logger.LogDebug($"Order saved with ID: {order.Id}. Cleared cart. Redirecting to ThankYouForYourOrder.");
            await SendNotificationEmailsToCustomerAndAdminUsersForNewOrderAsync(order.Id);
            await ClearBuyWithNoAccountCreationAsync(buyWithNoAccountCreation);
            await ClearCartAsync(shoppingCart);
            Logger.LogDebug("Cleared buyWithNoAccountCreation cart. Redirecting to ThankYouForYourOrder.");
            TempData[LastCompletedOrderIdKey] = order.Id;
            return RedirectToAction(ThankYouForYourOrderAction, new { orderId = order.Id });
        }

        private async Task ClearBuyWithNoAccountCreationAsync(BuyWithNoAccountCreation buyWithNoAccountCreation)
        {
            Logger.LogDebug($"Entering ClearBuyWithNoAccountCreationAsync with OrderGuid: {buyWithNoAccountCreation.OrderGuid}");
            await ShoppingCartService.DeleteByOrderGuidAsync(buyWithNoAccountCreation.OrderGuid);
            Logger.LogDebug("BuyNow cart deleted from data source.");
        }

        private async Task RevalidateCouponAsync(ShoppingCartSession shoppingCart)
        {
            if (shoppingCart?.Coupon == null || string.IsNullOrWhiteSpace(shoppingCart.Coupon.Code))
            {
                if (shoppingCart != null)
                {
                    shoppingCart.ClearValidatedCoupon();
                }
                return;
            }

            if (CouponValidationService == null)
            {
                try
                {
                    shoppingCart.Coupon = await CouponService.GetStorefrontCouponByCodeAsync(shoppingCart.Coupon.Code, CurrentLanguage);
                    if (shoppingCart.Coupon == null) shoppingCart.ClearValidatedCoupon();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Coupon revalidation failed; clearing coupon from cart.");
                    shoppingCart.ClearValidatedCoupon();
                }
                return;
            }

            try
            {
                var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null;
                var ctx = await BuildCouponValidationContextAsync(shoppingCart, userId).ConfigureAwait(false);
                // Use RevalidateActiveCoupon which ignores stacking for same coupon
                var result = await CouponValidationService.RevalidateActiveCouponAsync(shoppingCart, ctx).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    Logger.LogWarning($"Coupon revalidation failed: {result.Reason} - {result.Message}. Removing coupon.");
                    shoppingCart.ClearValidatedCoupon();
                    TempData["CouponMessage"] = $"Coupon removed: {result.Message}";
                    TempData["CouponMessageType"] = "warning";
                }
                else
                {
                    // Update validated amounts (covers price changes, sale status changes, etc.)
                    shoppingCart.SetValidatedCouponDiscount(result.DiscountAmount, result.ShippingDiscount, result.EligibleAmount);
                    // Refresh coupon DTO to latest (e.g., discount values may have changed)
                    try
                    {
                        var freshDto = await CouponService.GetStorefrontCouponByCodeAsync(shoppingCart.Coupon.Code, CurrentLanguage);
                        if (freshDto != null) shoppingCart.Coupon = freshDto;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Coupon revalidation failed; clearing coupon from cart.");
                shoppingCart.ClearValidatedCoupon();
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
                StaticPaymentLogger.LogError("Payment callback orderGuid is empty.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!string.Equals(paymentResult.BasketId, orderGuid, StringComparison.OrdinalIgnoreCase))
            {
                StaticPaymentLogger.LogError("Payment BasketId does not match callback order reference.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(orderNumber)
                && !string.IsNullOrWhiteSpace(paymentResult.ConversationId)
                && !string.Equals(paymentResult.ConversationId, orderNumber, StringComparison.OrdinalIgnoreCase))
            {
                StaticPaymentLogger.LogError("Payment ConversationId does not match callback orderNumber.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return null;
        }

        private async Task<Order> FindExistingPaidOrderAsync(string paymentId, string orderGuid)
        {
            var byPaymentId = await OrderService.GetByPaymentIdAsync(paymentId);
            if (byPaymentId != null)
            {
                Logger.LogInformation($"Idempotent payment callback: existing order {byPaymentId.Id} for PaymentId.");
                return byPaymentId;
            }

            if (!string.IsNullOrWhiteSpace(orderGuid))
            {
                var byOrderGuid = await OrderService.GetByOrderGuidAsync(orderGuid);
                if (byOrderGuid != null
                    && !string.IsNullOrEmpty(byOrderGuid.PaymentId)
                    && string.Equals(byOrderGuid.PaymentStatus, Domain.Constants.SUCCESS, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogInformation($"Idempotent payment callback: existing order {byOrderGuid.Id} for OrderGuid.");
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
