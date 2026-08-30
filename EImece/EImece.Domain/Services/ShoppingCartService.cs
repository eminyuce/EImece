using EImece.Domain.DependencyInjection;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework;
using EImece.Domain.Helpers;
using EImece.Domain.Models;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Newtonsoft.Json;
using EImece.Domain.Observability.Telemetry;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ShoppingCartService : BaseEntityService<ShoppingCart>, IShoppingCartService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOrderService OrderService;

        private readonly ICustomerService CustomerService;

        private readonly IAddressService AddressService;

        private readonly IOrderProductService OrderProductService;

        private readonly IProductService ProductService;

        private readonly IShoppingCartRepository ShoppingCartRepository;
        public ApplicationUserManager UserManager { get; }
        private readonly ICouponValidationService CouponValidationService;
        private readonly ICouponRedemptionRepository CouponRedemptionRepository;

        public ShoppingCartService(
            ApplicationUserManager userManager,
            IShoppingCartRepository repository,
            IOrderService orderService,
            ICustomerService customerService,
            IAddressService addressService,
            IOrderProductService orderProductService,
            IProductService productService,
            ICouponValidationService couponValidationService = null,
            ICouponRedemptionRepository couponRedemptionRepository = null) : base(repository)
        {
            Logger.Debug("ShoppingCartService initialized");
            this.ShoppingCartRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.UserManager = userManager;
            this.OrderService = orderService;
            this.CustomerService = customerService;
            this.AddressService = addressService;
            this.OrderProductService = orderProductService;
            this.ProductService = productService;
            this.CouponValidationService = couponValidationService;
            this.CouponRedemptionRepository = couponRedemptionRepository;
        }

        [Timed("service.shopping_cart.save_or_edit_sync")]
        public virtual void SaveOrEditShoppingCart(ShoppingCart item)
        {
            Logger.Debug($"SaveOrEditShoppingCart called with OrderGuid: {item.OrderGuid}");
            var shoppingCart = ShoppingCartRepository.GetShoppingCartByOrderGuid(item.OrderGuid);
            if (shoppingCart == null)
            {
                Logger.Debug($"No existing shopping cart found for OrderGuid: {item.OrderGuid}. Creating new cart.");
                shoppingCart = item;
            }
            else
            {
                Logger.Debug($"Existing shopping cart found for OrderGuid: {item.OrderGuid}. Updating cart content.");
                shoppingCart.ShoppingCartJson = item.ShoppingCartJson;
            }
            ShoppingCartRepository.SaveOrEdit(shoppingCart);
            Logger.Debug($"Shopping cart saved successfully for OrderGuid: {item.OrderGuid}");
        }

        [Timed("service.shopping_cart.save_or_edit")]
        public virtual async Task SaveOrEditShoppingCartAsync(ShoppingCart item)
        {
            Logger.Debug($"SaveOrEditShoppingCartAsync called with OrderGuid: {item.OrderGuid}");
            var shoppingCart = await ShoppingCartRepository.GetShoppingCartByOrderGuidAsync(item.OrderGuid).ConfigureAwait(false);
            if (shoppingCart == null)
            {
                Logger.Debug($"No existing shopping cart found for OrderGuid: {item.OrderGuid}. Creating new cart.");
                shoppingCart = item;
            }
            else
            {
                Logger.Debug($"Existing shopping cart found for OrderGuid: {item.OrderGuid}. Updating cart content.");
                shoppingCart.ShoppingCartJson = item.ShoppingCartJson;
            }
            await ShoppingCartRepository.SaveOrEditAsync(shoppingCart).ConfigureAwait(false);
            Logger.Debug($"Shopping cart saved successfully for OrderGuid: {item.OrderGuid}");
        }

        [Timed("service.shopping_cart.get_by_order_guid_sync")]
        public virtual ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            Logger.Debug($"GetShoppingCartByOrderGuid called with OrderGuid: {orderGuid}");
            var cart = ShoppingCartRepository.GetShoppingCartByOrderGuid(orderGuid);
            Logger.Debug($"GetShoppingCartByOrderGuid result: {(cart == null ? "No cart found" : "Cart found")}");
            return cart;
        }

        [Timed("service.shopping_cart.get_by_order_guid")]
        public virtual async Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid)
        {
            Logger.Debug($"GetShoppingCartByOrderGuidAsync called with OrderGuid: {orderGuid}");
            var cart = await ShoppingCartRepository.GetShoppingCartByOrderGuidAsync(orderGuid).ConfigureAwait(false);
            Logger.Debug($"GetShoppingCartByOrderGuidAsync result: {(cart == null ? "No cart found" : "Cart found")}");
            return cart;
        }

        [Timed("service.shopping_cart.delete_by_order_guid_sync")]
        public virtual void DeleteByOrderGuid(string orderGuid)
        {
            Logger.Debug($"DeleteByOrderGuid called with OrderGuid: {orderGuid}");
            ShoppingCartRepository.DeleteByWhereCondition(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase));
            Logger.Debug($"Shopping cart deleted for OrderGuid: {orderGuid}");
        }

        [Timed("service.shopping_cart.delete_by_order_guid")]
        public virtual async Task DeleteByOrderGuidAsync(string orderGuid)
        {
            Logger.Debug($"DeleteByOrderGuidAsync called with OrderGuid: {orderGuid}");
            var carts = await ShoppingCartRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).ToListAsync().ConfigureAwait(false);
            foreach (var cart in carts)
            {
                ShoppingCartRepository.Delete(cart);
            }
            if (carts.Count > 0)
            {
                await ShoppingCartRepository.SaveAsync().ConfigureAwait(false);
            }
            Logger.Debug($"Shopping cart deleted for OrderGuid: {orderGuid}");
        }

        [Timed("service.shopping_cart.save_cart_sync")]
        public virtual Order SaveShoppingCart(string orderNumber, ShoppingCartSession shoppingCart, PaymentResult paymentResult, string userId)
        {
            Logger.Debug($"SaveShoppingCart started - UserId: {userId}, OrderGuid: {shoppingCart?.OrderGuid}");

            if (shoppingCart == null)
            {
                Logger.Error("SaveShoppingCart failed: ShoppingCartSession is null");
                throw new ArgumentNullException(nameof(shoppingCart), "ShoppingCartSession is null");
            }
            if (paymentResult == null)
            {
                Logger.Error("SaveShoppingCart failed: PaymentResult is null");
                throw new ArgumentNullException(nameof(paymentResult), "PaymentResult is null");
            }
            if (string.IsNullOrEmpty(userId))
            {
                Logger.Error("SaveShoppingCart failed: userId is null or empty");
                throw new ArgumentNullException(nameof(userId), "userId is null");
            }
            CouponValidationResult couponValidationResult = null;
            // Use Serializable for coupon redemption race safety
            using (var transaction = ShoppingCartRepository.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    Logger.Debug($"Processing addresses - Initial ShippingAddressId: {shoppingCart.ShippingAddress.Id}, BillingAddressId: {shoppingCart.BillingAddress.Id}");

                    int shippingAddressId = shoppingCart.ShippingAddress.Id;
                    int billingAddressId = shoppingCart.BillingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Debug("Creating new shipping address");
                        shoppingCart.ShippingAddress.Name = Resource.ShippingAddress;
                        shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shoppingCart.ShippingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                        shoppingCart.ShippingAddress.City = shoppingCart.Customer.City.ToStr();
                        shoppingCart.ShippingAddress.Country = shoppingCart.Customer.Country.ToStr();
                        shoppingCart.ShippingAddress.ZipCode = shoppingCart.Customer.ZipCode.ToStr();
                        var shippingAddress = AddressService.SaveOrEditEntity(shoppingCart.ShippingAddress.ToEntity());
                        shippingAddressId = shippingAddress.Id;
                        Logger.Debug($"New shipping address created with Id: {shippingAddressId}");
                    }
                    if (billingAddressId == 0)
                    {
                        Logger.Debug("Creating new billing address");
                        shoppingCart.BillingAddress.Name = Resource.BillingAdress;
                        shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                        shoppingCart.BillingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                        shoppingCart.BillingAddress.City = shoppingCart.Customer.City.ToStr();
                        shoppingCart.BillingAddress.Country = shoppingCart.Customer.Country.ToStr();
                        shoppingCart.BillingAddress.ZipCode = shoppingCart.Customer.ZipCode.ToStr();
                        var billingAddress = AddressService.SaveOrEditEntity(shoppingCart.BillingAddress.ToEntity());
                        billingAddressId = billingAddress.Id;
                        Logger.Debug($"New billing address created with Id: {billingAddressId}");
                    }

                    Logger.Debug($"Saving customer type to normal for userId: {userId}");
                    CustomerService.SaveCustomerTypeToNormal(userId);

                    // Central coupon validation (must happen inside transaction before order creation)
                    couponValidationResult = ValidateCouponForOrderSync(shoppingCart, userId, paymentResult.Currency);

                    Logger.Debug($"Creating order for userId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");
                    Order savedOrder = SaveOrder(orderNumber, userId, shoppingCart, paymentResult, shippingAddressId, billingAddressId, couponValidationResult);
                    Logger.Debug($"Order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Debug($"Saving order products for OrderId: {savedOrder.Id}");
                    SaveOrderProduct(shoppingCart, savedOrder);
                    Logger.Debug($"Order products saved successfully for OrderId: {savedOrder.Id}");

                    // Record coupon redemption transactionally (after order and products)
                    if (couponValidationResult != null && couponValidationResult.IsValid && couponValidationResult.CouponId.HasValue)
                    {
                        RecordCouponRedemptionSync(couponValidationResult, savedOrder, userId, shoppingCart);
                        Logger.Info($"Coupon redemption recorded for CouponId: {couponValidationResult.CouponId}, OrderId: {savedOrder.Id}");
                    }

                    transaction?.Commit();

                    Logger.Debug($"SaveShoppingCart completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
                    return savedOrder;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Logger.Error(ex, $"SaveShoppingCart failed and rolled back for OrderGuid: {shoppingCart.OrderGuid}");
                    throw;
                }
            }
        }

        [Timed("service.shopping_cart.save_cart")]
        public virtual async Task<Order> SaveShoppingCartAsync(string orderNumber, ShoppingCartSession shoppingCart, PaymentResult paymentResult, string userId)
        {
            Logger.Debug($"SaveShoppingCartAsync started - UserId: {userId}, OrderGuid: {shoppingCart?.OrderGuid}");

            if (shoppingCart == null)
            {
                Logger.Error("SaveShoppingCartAsync failed: ShoppingCartSession is null");
                throw new ArgumentNullException(nameof(shoppingCart), "ShoppingCartSession is null");
            }
            if (paymentResult == null)
            {
                Logger.Error("SaveShoppingCartAsync failed: PaymentResult is null");
                throw new ArgumentNullException(nameof(paymentResult), "PaymentResult is null");
            }
            if (string.IsNullOrEmpty(userId))
            {
                Logger.Error("SaveShoppingCartAsync failed: userId is null or empty");
                throw new ArgumentNullException(nameof(userId), "userId is null");
            }
            CouponValidationResult couponValidationResultAsync = null;
            using (var transaction = ShoppingCartRepository.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    Logger.Debug($"Processing addresses - Initial ShippingAddressId: {shoppingCart.ShippingAddress.Id}, BillingAddressId: {shoppingCart.BillingAddress.Id}");

                    int shippingAddressId = shoppingCart.ShippingAddress.Id;
                    int billingAddressId = shoppingCart.BillingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        shoppingCart.ShippingAddress.Name = Resource.ShippingAddress;
                        shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shoppingCart.ShippingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                        shoppingCart.ShippingAddress.City = shoppingCart.Customer.City.ToStr();
                        shoppingCart.ShippingAddress.Country = shoppingCart.Customer.Country.ToStr();
                        shoppingCart.ShippingAddress.ZipCode = shoppingCart.Customer.ZipCode.ToStr();
                        var shippingAddress = await AddressService.SaveOrEditEntityAsync(shoppingCart.ShippingAddress.ToEntity()).ConfigureAwait(false);
                        shippingAddressId = shippingAddress.Id;
                    }
                    if (billingAddressId == 0)
                    {
                        shoppingCart.BillingAddress.Name = Resource.BillingAdress;
                        shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                        shoppingCart.BillingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                        shoppingCart.BillingAddress.City = shoppingCart.Customer.City.ToStr();
                        shoppingCart.BillingAddress.Country = shoppingCart.Customer.Country.ToStr();
                        shoppingCart.BillingAddress.ZipCode = shoppingCart.Customer.ZipCode.ToStr();
                        var billingAddress = await AddressService.SaveOrEditEntityAsync(shoppingCart.BillingAddress.ToEntity()).ConfigureAwait(false);
                        billingAddressId = billingAddress.Id;
                    }

                    await CustomerService.SaveCustomerTypeToNormalAsync(userId).ConfigureAwait(false);

                    couponValidationResultAsync = await ValidateCouponForOrderAsync(shoppingCart, userId, paymentResult.Currency).ConfigureAwait(false);

                    Logger.Debug($"Creating order for userId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");
                    Order savedOrder = await SaveOrderAsync(orderNumber, userId, shoppingCart, paymentResult, shippingAddressId, billingAddressId, couponValidationResultAsync).ConfigureAwait(false);
                    Logger.Debug($"Order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    await SaveOrderProductAsync(shoppingCart, savedOrder).ConfigureAwait(false);
                    Logger.Debug($"Order products saved successfully for OrderId: {savedOrder.Id}");

                    if (couponValidationResultAsync != null && couponValidationResultAsync.IsValid && couponValidationResultAsync.CouponId.HasValue)
                    {
                        await RecordCouponRedemptionAsync(couponValidationResultAsync, savedOrder, userId, shoppingCart).ConfigureAwait(false);
                        Logger.Info($"Coupon redemption recorded for CouponId: {couponValidationResultAsync.CouponId}, OrderId: {savedOrder.Id}");
                    }

                    transaction?.Commit();

                    Logger.Debug($"SaveShoppingCartAsync completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
                    return savedOrder;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Logger.Error(ex, $"SaveShoppingCartAsync failed and rolled back for OrderGuid: {shoppingCart.OrderGuid}");
                    throw;
                }
            }
        }

        private Order SaveOrder(string orderNumber, String userId, ShoppingCartSession shoppingCart, PaymentResult paymentResult,
            int shippingAddressId,
           int billingAddressId, CouponValidationResult couponValidation = null)
        {
            Logger.Debug($"SaveOrder started - UserId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");

            if (shippingAddressId == 0)
            {
                Logger.Error("SaveOrder failed: shippingAddressId is 0");
                throw new ArgumentNullException("shippingAddressId", "shippingAddressId is 0");
            }
            if (billingAddressId == 0)
            {
                Logger.Error("SaveOrder failed: billingAddressId is 0");
                throw new ArgumentNullException("billingAddressId", "billingAddressId is 0");
            }

            var item = new Order();
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = billingAddressId;
            item.OrderComments = shoppingCart.OrderComments;
            item.Name = shoppingCart.Customer.FullName;
            item.OrderGuid = shoppingCart.OrderGuid;
            item.OrderType = (int)EImeceOrderType.NormalOrder;
            item.OrderNumber = orderNumber;
            // Apply coupon validated shipping discount (free shipping) before order cargo
            decimal validatedCouponDiscount = 0;
            decimal validatedShippingDiscount = 0;
            string validatedCouponCode = "";
            if (couponValidation != null && couponValidation.IsValid)
            {
                validatedCouponDiscount = couponValidation.DiscountAmount;
                validatedShippingDiscount = couponValidation.ShippingDiscount;
                validatedCouponCode = couponValidation.CouponCode;
            }
            else if (shoppingCart.Coupon != null)
            {
                validatedCouponCode = shoppingCart.Coupon.Code;
                validatedCouponDiscount = shoppingCart.CalculateCouponDiscount(shoppingCart.TotalPrice);
            }
            // Cargo price with free shipping discount applied never negative
            decimal cargoVal = shoppingCart.CargoPriceValue;
            if (validatedShippingDiscount > 0) cargoVal = 0;
            item.CargoPrice = cargoVal;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = validatedCouponCode;
            item.CouponDiscount = validatedCouponDiscount.CurrencySignForIyizo();
            if (validatedCouponDiscount > 0) item.AdminOrderNote = (item.AdminOrderNote ?? "") + $" Coupon:{validatedCouponCode} Discount:{validatedCouponDiscount} ";
            item.Token = paymentResult.Token;
            item.Price = paymentResult.Price;
            item.PaidPrice = paymentResult.PaidPrice;
            item.Installment = paymentResult.Installment ?? "";
            item.Currency = paymentResult.Currency;
            item.PaymentId = paymentResult.PaymentId;
            item.PaymentStatus = paymentResult.PaymentStatus;
            item.FraudStatus = paymentResult.FraudStatus;
            item.MerchantCommissionRate = paymentResult.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = paymentResult.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = paymentResult.IyziCommissionRateAmount;
            item.IyziCommissionFee = paymentResult.IyziCommissionFee;
            item.CardType = paymentResult.CardType;
            item.CardAssociation = paymentResult.CardAssociation;
            item.CardFamily = paymentResult.CardFamily;
            item.CardToken = paymentResult.CardToken;
            item.CardUserKey = paymentResult.CardUserKey;
            item.BinNumber = paymentResult.BinNumber;
            item.LastFourDigits = paymentResult.LastFourDigits;
            item.BasketId = paymentResult.BasketId;
            item.ConversationId = paymentResult.ConversationId;
            item.ConnectorName = paymentResult.ConnectorName;
            item.AuthCode = paymentResult.AuthCode;
            item.HostReference = paymentResult.HostReference;
            item.Phase = paymentResult.Phase;
            item.Status = paymentResult.Status;
            item.ErrorCode = paymentResult.ErrorCode;
            item.ErrorMessage = paymentResult.ErrorMessage;
            item.Locale = paymentResult.Locale;
            item.SystemTime = paymentResult.SystemTime;

            Logger.Debug($"Saving order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            Logger.Info($"Order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private Order SaveOrder(String orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult,
          int shippingAddressId, CouponValidationResult couponValidation = null)
        {
            Logger.Debug($"SaveOrder (buyWithNoAccountCreation) started - UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");

            var item = new Order();

            item.OrderComments = buyWithNoAccountCreation.OrderComments;
            item.Name = buyWithNoAccountCreation.Customer.FullName;
            item.OrderGuid = buyWithNoAccountCreation.OrderGuid;
            item.OrderType = (int)EImeceOrderType.BuyWithNoAccountCreation;
            item.OrderNumber = orderNumber;
            decimal guestCouponDisc = 0;
            decimal guestShippingDisc = 0;
            string guestCouponCode = "";
            if (couponValidation != null && couponValidation.IsValid)
            {
                guestCouponDisc = couponValidation.DiscountAmount;
                guestShippingDisc = couponValidation.ShippingDiscount;
                guestCouponCode = couponValidation.CouponCode;
            }
            else if (buyWithNoAccountCreation.Coupon != null)
            {
                guestCouponCode = buyWithNoAccountCreation.Coupon.Code;
                guestCouponDisc = buyWithNoAccountCreation.CalculateCouponDiscount(buyWithNoAccountCreation.TotalPrice);
            }
            decimal cargoG = buyWithNoAccountCreation.CargoPriceValue;
            if (guestShippingDisc > 0) cargoG = 0;
            item.CargoPrice = cargoG;
            item.UserId = buyWithNoAccountCreation.Customer.UserId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = guestCouponCode;
            item.CouponDiscount = guestCouponDisc.CurrencySignForIyizo();
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = shippingAddressId; // Billing currently shares the shipping address id.
            item.Token = paymentResult.Token;
            item.Price = paymentResult.Price;
            item.PaidPrice = paymentResult.PaidPrice;
            item.Installment = paymentResult.Installment ?? "";
            item.Currency = paymentResult.Currency;
            item.PaymentId = paymentResult.PaymentId;
            item.PaymentStatus = paymentResult.PaymentStatus;
            item.FraudStatus = paymentResult.FraudStatus;
            item.MerchantCommissionRate = paymentResult.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = paymentResult.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = paymentResult.IyziCommissionRateAmount;
            item.IyziCommissionFee = paymentResult.IyziCommissionFee;
            item.CardType = paymentResult.CardType;
            item.CardAssociation = paymentResult.CardAssociation;
            item.CardFamily = paymentResult.CardFamily;
            item.CardToken = paymentResult.CardToken;
            item.CardUserKey = paymentResult.CardUserKey;
            item.BinNumber = paymentResult.BinNumber;
            item.LastFourDigits = paymentResult.LastFourDigits;
            item.BasketId = paymentResult.BasketId;
            item.ConversationId = paymentResult.ConversationId;
            item.ConnectorName = paymentResult.ConnectorName;
            item.AuthCode = paymentResult.AuthCode;
            item.HostReference = paymentResult.HostReference;
            item.Phase = paymentResult.Phase;
            item.Status = paymentResult.Status;
            item.ErrorCode = paymentResult.ErrorCode;
            item.ErrorMessage = paymentResult.ErrorMessage;
            item.Locale = paymentResult.Locale;
            item.SystemTime = paymentResult.SystemTime;

            Logger.Debug($"Saving buyWithNoAccountCreation order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            Logger.Info($"buyWithNoAccountCreation order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private Order SaveOrder(String userId, BuyNowModel buyNowSession, PaymentResult paymentResult,
          int shippingAddressId)
        {
            Logger.Debug($"SaveOrder (BuyNow) started - UserId: {userId}, ShippingAddressId: {shippingAddressId}");

            var item = new Order();

            item.OrderComments = "";
            item.Name = buyNowSession.Customer.FullName;
            item.OrderGuid = buyNowSession.OrderGuid;
            item.OrderType = (int)EImeceOrderType.BuyNow;
            item.OrderNumber = GeneralHelper.RandomNumber(12);
            item.CargoPrice = buyNowSession.CargoPriceValue;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = 1;
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = shippingAddressId;
            item.Coupon = "";
            item.Token = paymentResult.Token;
            item.Price = paymentResult.Price;
            item.PaidPrice = paymentResult.PaidPrice;
            item.Installment = paymentResult.Installment ?? "";
            item.Currency = paymentResult.Currency;
            item.PaymentId = paymentResult.PaymentId;
            item.PaymentStatus = paymentResult.PaymentStatus;
            item.FraudStatus = paymentResult.FraudStatus;
            item.MerchantCommissionRate = paymentResult.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = paymentResult.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = paymentResult.IyziCommissionRateAmount;
            item.IyziCommissionFee = paymentResult.IyziCommissionFee;
            item.CardType = paymentResult.CardType;
            item.CardAssociation = paymentResult.CardAssociation;
            item.CardFamily = paymentResult.CardFamily;
            item.CardToken = paymentResult.CardToken;
            item.CardUserKey = paymentResult.CardUserKey;
            item.BinNumber = paymentResult.BinNumber;
            item.LastFourDigits = paymentResult.LastFourDigits;
            item.BasketId = paymentResult.BasketId;
            item.ConversationId = paymentResult.ConversationId;
            item.ConnectorName = paymentResult.ConnectorName;
            item.AuthCode = paymentResult.AuthCode;
            item.HostReference = paymentResult.HostReference;
            item.Phase = paymentResult.Phase;
            item.Status = paymentResult.Status;
            item.ErrorCode = paymentResult.ErrorCode;
            item.ErrorMessage = paymentResult.ErrorMessage;
            item.Locale = paymentResult.Locale;
            item.SystemTime = paymentResult.SystemTime;

            Logger.Debug($"Saving BuyNow order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            Logger.Info($"BuyNow order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private async Task<Order> SaveOrderAsync(string orderNumber, String userId, ShoppingCartSession shoppingCart, PaymentResult paymentResult,
            int shippingAddressId,
           int billingAddressId, CouponValidationResult couponValidation = null)
        {
            Logger.Debug($"SaveOrderAsync started - UserId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");

            if (shippingAddressId == 0)
            {
                Logger.Error("SaveOrderAsync failed: shippingAddressId is 0");
                throw new ArgumentNullException("shippingAddressId", "shippingAddressId is 0");
            }
            if (billingAddressId == 0)
            {
                Logger.Error("SaveOrderAsync failed: billingAddressId is 0");
                throw new ArgumentNullException("billingAddressId", "billingAddressId is 0");
            }

            var item = new Order();
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = billingAddressId;
            item.OrderComments = shoppingCart.OrderComments;
            item.Name = shoppingCart.Customer.FullName;
            item.OrderGuid = shoppingCart.OrderGuid;
            item.OrderType = (int)EImeceOrderType.NormalOrder;
            item.OrderNumber = orderNumber;
            decimal validatedCouponDiscountAsync = 0;
            decimal validatedShippingDiscountAsync = 0;
            string validatedCouponCodeAsync = "";
            if (couponValidation != null && couponValidation.IsValid)
            {
                validatedCouponDiscountAsync = couponValidation.DiscountAmount;
                validatedShippingDiscountAsync = couponValidation.ShippingDiscount;
                validatedCouponCodeAsync = couponValidation.CouponCode;
            }
            else if (shoppingCart.Coupon != null)
            {
                validatedCouponCodeAsync = shoppingCart.Coupon.Code;
                validatedCouponDiscountAsync = shoppingCart.CalculateCouponDiscount(shoppingCart.TotalPrice);
            }
            decimal cargoValAsync = shoppingCart.CargoPriceValue;
            if (validatedShippingDiscountAsync > 0) cargoValAsync = 0;
            item.CargoPrice = cargoValAsync;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = validatedCouponCodeAsync;
            item.CouponDiscount = validatedCouponDiscountAsync.CurrencySignForIyizo();
            if (validatedCouponDiscountAsync > 0) item.AdminOrderNote = (item.AdminOrderNote ?? "") + $" Coupon:{validatedCouponCodeAsync} Discount:{validatedCouponDiscountAsync} ";
            item.Token = paymentResult.Token;
            item.Price = paymentResult.Price;
            item.PaidPrice = paymentResult.PaidPrice;
            item.Installment = paymentResult.Installment ?? "";
            item.Currency = paymentResult.Currency;
            item.PaymentId = paymentResult.PaymentId;
            item.PaymentStatus = paymentResult.PaymentStatus;
            item.FraudStatus = paymentResult.FraudStatus;
            item.MerchantCommissionRate = paymentResult.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = paymentResult.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = paymentResult.IyziCommissionRateAmount;
            item.IyziCommissionFee = paymentResult.IyziCommissionFee;
            item.CardType = paymentResult.CardType;
            item.CardAssociation = paymentResult.CardAssociation;
            item.CardFamily = paymentResult.CardFamily;
            item.CardToken = paymentResult.CardToken;
            item.CardUserKey = paymentResult.CardUserKey;
            item.BinNumber = paymentResult.BinNumber;
            item.LastFourDigits = paymentResult.LastFourDigits;
            item.BasketId = paymentResult.BasketId;
            item.ConversationId = paymentResult.ConversationId;
            item.ConnectorName = paymentResult.ConnectorName;
            item.AuthCode = paymentResult.AuthCode;
            item.HostReference = paymentResult.HostReference;
            item.Phase = paymentResult.Phase;
            item.Status = paymentResult.Status;
            item.ErrorCode = paymentResult.ErrorCode;
            item.ErrorMessage = paymentResult.ErrorMessage;
            item.Locale = paymentResult.Locale;
            item.SystemTime = paymentResult.SystemTime;

            Logger.Debug($"Saving order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = await OrderService.SaveOrEditEntityAsync(item).ConfigureAwait(false);
            Logger.Info($"Order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private async Task<Order> SaveOrderAsync(String orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult,
          int shippingAddressId, CouponValidationResult couponValidation = null)
        {
            Logger.Debug($"SaveOrderAsync (buyWithNoAccountCreation) started - UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");

            var item = new Order();

            item.OrderComments = buyWithNoAccountCreation.OrderComments;
            item.Name = buyWithNoAccountCreation.Customer.FullName;
            item.OrderGuid = buyWithNoAccountCreation.OrderGuid;
            item.OrderType = (int)EImeceOrderType.BuyWithNoAccountCreation;
            item.OrderNumber = orderNumber;
            decimal guestDiscAsync = 0;
            decimal guestShipAsync = 0;
            string guestCodeAsync = "";
            if (couponValidation != null && couponValidation.IsValid)
            {
                guestDiscAsync = couponValidation.DiscountAmount;
                guestShipAsync = couponValidation.ShippingDiscount;
                guestCodeAsync = couponValidation.CouponCode;
            }
            else if (buyWithNoAccountCreation.Coupon != null)
            {
                guestCodeAsync = buyWithNoAccountCreation.Coupon.Code;
                guestDiscAsync = buyWithNoAccountCreation.CalculateCouponDiscount(buyWithNoAccountCreation.TotalPrice);
            }
            decimal cargoGAsync = buyWithNoAccountCreation.CargoPriceValue;
            if (guestShipAsync > 0) cargoGAsync = 0;
            item.CargoPrice = cargoGAsync;
            item.UserId = buyWithNoAccountCreation.Customer.UserId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = guestCodeAsync;
            item.CouponDiscount = guestDiscAsync.CurrencySignForIyizo();
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = shippingAddressId; // Billing currently shares the shipping address id.
            item.Token = paymentResult.Token;
            item.Price = paymentResult.Price;
            item.PaidPrice = paymentResult.PaidPrice;
            item.Installment = paymentResult.Installment ?? "";
            item.Currency = paymentResult.Currency;
            item.PaymentId = paymentResult.PaymentId;
            item.PaymentStatus = paymentResult.PaymentStatus;
            item.FraudStatus = paymentResult.FraudStatus;
            item.MerchantCommissionRate = paymentResult.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = paymentResult.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = paymentResult.IyziCommissionRateAmount;
            item.IyziCommissionFee = paymentResult.IyziCommissionFee;
            item.CardType = paymentResult.CardType;
            item.CardAssociation = paymentResult.CardAssociation;
            item.CardFamily = paymentResult.CardFamily;
            item.CardToken = paymentResult.CardToken;
            item.CardUserKey = paymentResult.CardUserKey;
            item.BinNumber = paymentResult.BinNumber;
            item.LastFourDigits = paymentResult.LastFourDigits;
            item.BasketId = paymentResult.BasketId;
            item.ConversationId = paymentResult.ConversationId;
            item.ConnectorName = paymentResult.ConnectorName;
            item.AuthCode = paymentResult.AuthCode;
            item.HostReference = paymentResult.HostReference;
            item.Phase = paymentResult.Phase;
            item.Status = paymentResult.Status;
            item.ErrorCode = paymentResult.ErrorCode;
            item.ErrorMessage = paymentResult.ErrorMessage;
            item.Locale = paymentResult.Locale;
            item.SystemTime = paymentResult.SystemTime;

            Logger.Debug($"Saving buyWithNoAccountCreation order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = await OrderService.SaveOrEditEntityAsync(item).ConfigureAwait(false);
            Logger.Info($"buyWithNoAccountCreation order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private async Task<Order> SaveOrderAsync(String userId, BuyNowModel buyNowSession, PaymentResult paymentResult,
          int shippingAddressId)
        {
            Logger.Debug($"SaveOrderAsync (BuyNow) started - UserId: {userId}, ShippingAddressId: {shippingAddressId}");

            var item = new Order();

            item.OrderComments = "";
            item.Name = buyNowSession.Customer.FullName;
            item.OrderGuid = buyNowSession.OrderGuid;
            item.OrderType = (int)EImeceOrderType.BuyNow;
            item.OrderNumber = GeneralHelper.RandomNumber(12);
            item.CargoPrice = buyNowSession.CargoPriceValue;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = 1;
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = shippingAddressId;
            item.Coupon = "";
            item.Token = paymentResult.Token;
            item.Price = paymentResult.Price;
            item.PaidPrice = paymentResult.PaidPrice;
            item.Installment = paymentResult.Installment ?? "";
            item.Currency = paymentResult.Currency;
            item.PaymentId = paymentResult.PaymentId;
            item.PaymentStatus = paymentResult.PaymentStatus;
            item.FraudStatus = paymentResult.FraudStatus;
            item.MerchantCommissionRate = paymentResult.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = paymentResult.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = paymentResult.IyziCommissionRateAmount;
            item.IyziCommissionFee = paymentResult.IyziCommissionFee;
            item.CardType = paymentResult.CardType;
            item.CardAssociation = paymentResult.CardAssociation;
            item.CardFamily = paymentResult.CardFamily;
            item.CardToken = paymentResult.CardToken;
            item.CardUserKey = paymentResult.CardUserKey;
            item.BinNumber = paymentResult.BinNumber;
            item.LastFourDigits = paymentResult.LastFourDigits;
            item.BasketId = paymentResult.BasketId;
            item.ConversationId = paymentResult.ConversationId;
            item.ConnectorName = paymentResult.ConnectorName;
            item.AuthCode = paymentResult.AuthCode;
            item.HostReference = paymentResult.HostReference;
            item.Phase = paymentResult.Phase;
            item.Status = paymentResult.Status;
            item.ErrorCode = paymentResult.ErrorCode;
            item.ErrorMessage = paymentResult.ErrorMessage;
            item.Locale = paymentResult.Locale;
            item.SystemTime = paymentResult.SystemTime;

            Logger.Debug($"Saving BuyNow order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = await OrderService.SaveOrEditEntityAsync(item).ConfigureAwait(false);
            Logger.Info($"BuyNow order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        [Timed("service.shopping_cart.save_buy_with_no_account_sync")]
        public virtual Order SaveBuyWithNoAccountCreation(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult)
        {
            Logger.Debug($"SaveBuyWithNoAccountCreation started - OrderGuid: {buyWithNoAccountCreation?.OrderGuid}");

            if (buyWithNoAccountCreation == null)
            {
                Logger.Error("buyWithNoAccountCreation failed: buyWithNoAccountCreation is null");
                throw new ArgumentNullException(nameof(buyWithNoAccountCreation), "buyWithNoAccountCreation is null");
            }
            if (paymentResult == null)
            {
                Logger.Error("SaveBuyWithNoAccountCreation failed: " + Constants.PaymentResultIsNullMessage);
                throw new ArgumentNullException(nameof(paymentResult), Constants.PaymentResultIsNullMessage);
            }
            CouponValidationResult guestValidation = null;
            using (var transaction = ShoppingCartRepository.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    CustomerDto customer = buyWithNoAccountCreation.Customer;
                    AddressDto shippingAddress = buyWithNoAccountCreation.ShippingAddress;
                    int shippingAddressId = shippingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Debug("Creating new shipping address for BuyWithNoAccountCreation order");
                        shippingAddress.Name = Resource.ShippingAddress;
                        shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shippingAddress.Description = customer.RegistrationAddress;
                        shippingAddress.City = customer.City;
                        shippingAddress.Country = customer.Country;
                        shippingAddress.ZipCode = customer.ZipCode;
                        var savedShippingAddress = AddressService.SaveOrEditEntity(shippingAddress.ToEntity());
                        shippingAddressId = savedShippingAddress.Id;
                        Logger.Debug($"New shipping address created with Id: {shippingAddressId}");
                    }

                    // Guest coupon validation (must enforce login-required, usage limits etc.)
                    if (buyWithNoAccountCreation.Coupon != null && !string.IsNullOrWhiteSpace(buyWithNoAccountCreation.Coupon.Code) && CouponValidationService != null)
                    {
                        try
                        {
                            var ctxGuest = new CouponValidationContext
                            {
                                UserId = buyWithNoAccountCreation.Customer.UserId,
                                IsAuthenticated = false,
                                Language = AppConfig.MainLanguage,
                                Currency = paymentResult.Currency,
                                CargoPrice = buyWithNoAccountCreation.CargoPriceValue,
                                HasExistingCoupon = false
                            };
                            guestValidation = CouponValidationService.ValidateCouponAsync(buyWithNoAccountCreation.Coupon.Code, buyWithNoAccountCreation, ctxGuest).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (!guestValidation.IsValid)
                            {
                                Logger.Warn($"Guest coupon validation failed: {guestValidation.Reason} - {guestValidation.Message}");
                                throw new InvalidOperationException($"{guestValidation.Reason}: {guestValidation.Message}");
                            }
                            buyWithNoAccountCreation.SetValidatedCouponDiscount(guestValidation.DiscountAmount, guestValidation.ShippingDiscount, guestValidation.EligibleAmount);
                        }
                        catch (InvalidOperationException) { throw; }
                        catch (Exception ex) { Logger.Error(ex, "Guest coupon validation failed"); throw new InvalidOperationException("Coupon validation failed: " + ex.Message, ex); }
                    }

                    Logger.Debug($"Creating buyWithNoAccountCreation order for UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = SaveOrder(orderNumber, buyWithNoAccountCreation, paymentResult, shippingAddressId, guestValidation);
                    Logger.Debug($"buyWithNoAccountCreation order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Debug($"Saving order product for buyWithNoAccountCreation OrderId: {savedOrder.Id}");
                    SaveOrderProduct(buyWithNoAccountCreation.ShoppingCartItems, savedOrder);
                    Logger.Debug($"Order product saved successfully for buyWithNoAccountCreation OrderId: {savedOrder.Id}");

                    if (guestValidation != null && guestValidation.IsValid && guestValidation.CouponId.HasValue)
                    {
                        // Record redemption for guest (use generated UserId as identifier, CustomerId null for guest)
                        var redemptionG = new CouponRedemption
                        {
                            Name = guestValidation.CouponCode,
                            CouponId = guestValidation.CouponId.Value,
                            OrderId = savedOrder.Id,
                            CustomerId = null,
                            UserId = buyWithNoAccountCreation.Customer.UserId,
                            CouponCode = guestValidation.CouponCode,
                            DiscountAmount = guestValidation.DiscountAmount,
                            OrderTotalBeforeDiscount = guestValidation.EligibleAmount,
                            Currency = savedOrder.Currency,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            IsActive = true,
                            Position = 0,
                            Lang = AppConfig.MainLanguage
                        };
                        if (CouponRedemptionRepository != null) CouponRedemptionRepository.SaveOrEdit(redemptionG);
                    }

                    transaction?.Commit();

                    Logger.Debug($"SaveBuyWithNoAccountCreation completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
                    return savedOrder;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Logger.Error(ex, $"SaveBuyWithNoAccountCreation failed and rolled back for OrderGuid: {buyWithNoAccountCreation.OrderGuid}");
                    throw;
                }
            }
        }

        [Timed("service.shopping_cart.save_buy_now_sync")]
        public virtual Order SaveBuyNow(BuyNowModel buyNowSession, PaymentResult paymentResult)
        {
            Logger.Debug($"SaveBuyNow started - OrderGuid: {buyNowSession?.OrderGuid}");

            if (buyNowSession == null)
            {
                Logger.Error("SaveBuyNow failed: buyNowSession is null");
                throw new ArgumentNullException(nameof(buyNowSession), "buyNowSession is null");
            }
            if (paymentResult == null)
            {
                Logger.Error("SaveBuyNow failed: " + Constants.PaymentResultIsNullMessage);
                throw new ArgumentNullException(nameof(paymentResult), Constants.PaymentResultIsNullMessage);
            }
            using (var transaction = ShoppingCartRepository.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    Logger.Debug("Saving customer information");
                    CustomerDto customer = buyNowSession.Customer;
                    customer.CustomerType = (int)EImeceCustomerType.BuyNow;
                    customer.CreatedDate = DateTime.Now;
                    customer.UpdatedDate = DateTime.Now;
                    var customerEntity = CustomerService.SaveOrEditEntity(customer.ToEntity());
                    Logger.Debug($"Customer saved with Id: {customerEntity.Id}");

                    buyNowSession.Customer.UserId = GeneralHelper.RandomNumber(12) + "-" + Constants.BuyNowCustomerUserId + "-" + customerEntity.Id;
                    Logger.Debug($"Generated UserId for BuyNow customer: {buyNowSession.Customer.UserId}");

                    AddressDto shippingAddress = buyNowSession.ShippingAddress;
                    int shippingAddressId = shippingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Debug("Creating new shipping address for BuyNow order");
                        shippingAddress.Name = Resource.ShippingAddress;
                        shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shippingAddress.Description = customer.RegistrationAddress;
                        shippingAddress.City = customer.City;
                        shippingAddress.Country = customer.Country;
                        shippingAddress.ZipCode = customer.ZipCode;
                        var savedShippingAddress = AddressService.SaveOrEditEntity(shippingAddress.ToEntity());
                        shippingAddressId = savedShippingAddress.Id;
                        Logger.Debug($"New shipping address created with Id: {shippingAddressId}");
                    }

                    Logger.Debug($"Creating BuyNow order for UserId: {buyNowSession.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = SaveOrder(buyNowSession.Customer.UserId, buyNowSession, paymentResult, shippingAddressId);
                    Logger.Debug($"BuyNow order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Debug($"Saving order product for BuyNow OrderId: {savedOrder.Id}");
                    SaveOrderProduct(buyNowSession, savedOrder);
                    Logger.Debug($"Order product saved successfully for BuyNow OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Debug($"SaveBuyNow completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
                    return savedOrder;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Logger.Error(ex, $"SaveBuyNow failed and rolled back for OrderGuid: {buyNowSession.OrderGuid}");
                    throw;
                }
            }
        }

        [Timed("service.shopping_cart.save_buy_with_no_account")]
        public virtual async Task<Order> SaveBuyWithNoAccountCreationAsync(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult)
        {
            Logger.Debug($"SaveBuyWithNoAccountCreationAsync started - OrderGuid: {buyWithNoAccountCreation?.OrderGuid}");

            if (buyWithNoAccountCreation == null)
            {
                Logger.Error("buyWithNoAccountCreation failed: buyWithNoAccountCreation is null");
                throw new ArgumentNullException(nameof(buyWithNoAccountCreation), "buyWithNoAccountCreation is null");
            }
            if (paymentResult == null)
            {
                Logger.Error("SaveBuyWithNoAccountCreationAsync failed: " + Constants.PaymentResultIsNullMessage);
                throw new ArgumentNullException(nameof(paymentResult), Constants.PaymentResultIsNullMessage);
            }
            CouponValidationResult guestValidationAsync = null;
            using (var transaction = ShoppingCartRepository.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    CustomerDto customer = buyWithNoAccountCreation.Customer;
                    AddressDto shippingAddress = buyWithNoAccountCreation.ShippingAddress;
                    int shippingAddressId = shippingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Debug("Creating new shipping address for BuyWithNoAccountCreation order");
                        shippingAddress.Name = Resource.ShippingAddress;
                        shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shippingAddress.Description = customer.RegistrationAddress;
                        shippingAddress.City = customer.City;
                        shippingAddress.Country = customer.Country;
                        shippingAddress.ZipCode = customer.ZipCode;
                        var savedShippingAddress = await AddressService.SaveOrEditEntityAsync(shippingAddress.ToEntity()).ConfigureAwait(false);
                        shippingAddressId = savedShippingAddress.Id;
                        Logger.Debug($"New shipping address created with Id: {shippingAddressId}");
                    }

                    if (buyWithNoAccountCreation.Coupon != null && !string.IsNullOrWhiteSpace(buyWithNoAccountCreation.Coupon.Code) && CouponValidationService != null)
                    {
                        var ctxGuestAsync = new CouponValidationContext
                        {
                            UserId = buyWithNoAccountCreation.Customer.UserId,
                            IsAuthenticated = false,
                            Language = AppConfig.MainLanguage,
                            Currency = paymentResult.Currency,
                            CargoPrice = buyWithNoAccountCreation.CargoPriceValue,
                            HasExistingCoupon = false
                        };
                        guestValidationAsync = await CouponValidationService.ValidateCouponAsync(buyWithNoAccountCreation.Coupon.Code, buyWithNoAccountCreation, ctxGuestAsync).ConfigureAwait(false);
                        if (!guestValidationAsync.IsValid)
                        {
                            Logger.Warn($"Guest async coupon validation failed: {guestValidationAsync.Reason} - {guestValidationAsync.Message}");
                            throw new InvalidOperationException($"{guestValidationAsync.Reason}: {guestValidationAsync.Message}");
                        }
                        buyWithNoAccountCreation.SetValidatedCouponDiscount(guestValidationAsync.DiscountAmount, guestValidationAsync.ShippingDiscount, guestValidationAsync.EligibleAmount);
                    }

                    Logger.Debug($"Creating buyWithNoAccountCreation order for UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = await SaveOrderAsync(orderNumber, buyWithNoAccountCreation, paymentResult, shippingAddressId, guestValidationAsync).ConfigureAwait(false);
                    Logger.Debug($"buyWithNoAccountCreation order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Debug($"Saving order product for buyWithNoAccountCreation OrderId: {savedOrder.Id}");
                    await SaveOrderProductAsync(buyWithNoAccountCreation.ShoppingCartItems, savedOrder).ConfigureAwait(false);
                    Logger.Debug($"Order product saved successfully for buyWithNoAccountCreation OrderId: {savedOrder.Id}");

                    if (guestValidationAsync != null && guestValidationAsync.IsValid && guestValidationAsync.CouponId.HasValue)
                    {
                        var redemptionGAsync = new CouponRedemption
                        {
                            Name = guestValidationAsync.CouponCode,
                            CouponId = guestValidationAsync.CouponId.Value,
                            OrderId = savedOrder.Id,
                            CustomerId = null,
                            UserId = buyWithNoAccountCreation.Customer.UserId,
                            CouponCode = guestValidationAsync.CouponCode,
                            DiscountAmount = guestValidationAsync.DiscountAmount,
                            OrderTotalBeforeDiscount = guestValidationAsync.EligibleAmount,
                            Currency = savedOrder.Currency,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            IsActive = true,
                            Position = 0,
                            Lang = AppConfig.MainLanguage
                        };
                        if (CouponRedemptionRepository != null) await CouponRedemptionRepository.SaveOrEditAsync(redemptionGAsync).ConfigureAwait(false);
                    }

                    transaction?.Commit();

                    Logger.Debug($"SaveBuyWithNoAccountCreationAsync completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
                    return savedOrder;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Logger.Error(ex, $"SaveBuyWithNoAccountCreationAsync failed and rolled back for OrderGuid: {buyWithNoAccountCreation.OrderGuid}");
                    throw;
                }
            }
        }

        [Timed("service.shopping_cart.save_buy_now")]
        public virtual async Task<Order> SaveBuyNowAsync(BuyNowModel buyNowSession, PaymentResult paymentResult)
        {
            Logger.Debug($"SaveBuyNowAsync started - OrderGuid: {buyNowSession?.OrderGuid}");

            if (buyNowSession == null)
            {
                Logger.Error("SaveBuyNowAsync failed: buyNowSession is null");
                throw new ArgumentNullException(nameof(buyNowSession), "buyNowSession is null");
            }
            if (paymentResult == null)
            {
                Logger.Error("SaveBuyNowAsync failed: " + Constants.PaymentResultIsNullMessage);
                throw new ArgumentNullException(nameof(paymentResult), Constants.PaymentResultIsNullMessage);
            }
            using (var transaction = ShoppingCartRepository.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    Logger.Debug("Saving customer information");
                    CustomerDto customer = buyNowSession.Customer;
                    customer.CustomerType = (int)EImeceCustomerType.BuyNow;
                    customer.CreatedDate = DateTime.Now;
                    customer.UpdatedDate = DateTime.Now;
                    var customerEntity = await CustomerService.SaveOrEditEntityAsync(customer.ToEntity()).ConfigureAwait(false);

                    buyNowSession.Customer.UserId = GeneralHelper.RandomNumber(12) + "-" + Constants.BuyNowCustomerUserId + "-" + customerEntity.Id;

                    AddressDto shippingAddress = buyNowSession.ShippingAddress;
                    int shippingAddressId = shippingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        shippingAddress.Name = Resource.ShippingAddress;
                        shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shippingAddress.Description = customer.RegistrationAddress;
                        shippingAddress.City = customer.City;
                        shippingAddress.Country = customer.Country;
                        shippingAddress.ZipCode = customer.ZipCode;
                        var savedShippingAddress = await AddressService.SaveOrEditEntityAsync(shippingAddress.ToEntity()).ConfigureAwait(false);
                        shippingAddressId = savedShippingAddress.Id;
                    }

                    Logger.Debug($"Creating BuyNow order for UserId: {buyNowSession.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = await SaveOrderAsync(buyNowSession.Customer.UserId, buyNowSession, paymentResult, shippingAddressId).ConfigureAwait(false);
                    Logger.Debug($"BuyNow order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    await SaveOrderProductAsync(buyNowSession, savedOrder).ConfigureAwait(false);
                    Logger.Debug($"Order product saved successfully for BuyNow OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Debug($"SaveBuyNowAsync completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
                    return savedOrder;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Logger.Error(ex, $"SaveBuyNowAsync failed and rolled back for OrderGuid: {buyNowSession.OrderGuid}");
                    throw;
                }
            }
        }

        private void SaveOrderProduct(ShoppingCartSession shoppingCart, Order savedOrder)
        {
            SaveOrderProduct(shoppingCart.ShoppingCartItems, savedOrder);
        }

        private void SaveOrderProduct(List<ShoppingCartItem> shoppingCartItems, Order savedOrder)
        {
            Logger.Debug($"SaveOrderProduct started for OrderId: {savedOrder.Id}, ItemCount: {shoppingCartItems.Count}");

            foreach (var shoppingCartItem in shoppingCartItems)
            {
                var product = shoppingCartItem.Product;
                Logger.Debug($"Saving order product - OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}, Quantity: {shoppingCartItem.Quantity}");

                OrderProductService.SaveOrEditEntity(new OrderProduct()
                {
                    OrderId = savedOrder.Id,
                    ProductId = product.Id > 0 ? (int?)product.Id : null,
                    ProductSalePrice = product.Price,
                    ProductName = product.Name,
                    ProductCode = product.ProductCode,
                    CategoryName = product.CategoryName,
                    Quantity = shoppingCartItem.Quantity,
                    TotalPrice = shoppingCartItem.TotalPrice,
                    ProductSpecItems = JsonConvert.SerializeObject(product.ProductSpecItems),
                    ProductImageUrl = product.CroppedImageUrl
                });

                if (ProductService != null && product.Id > 0)
                {
                    ProductService.DecreaseStock(product.Id, shoppingCartItem.Quantity);
                }

                Logger.Debug($"Order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}");
            }

            Logger.Debug($"All order products saved successfully for OrderId: {savedOrder.Id}");
        }

        private void SaveOrderProduct(BuyNowModel buyNowModel, Order savedOrder)
        {
            var product = buyNowModel.ShoppingCartItem.Product;
            Logger.Debug($"SaveOrderProduct (BuyNow) started for OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}");

            var entity = new OrderProduct()
            {
                OrderId = savedOrder.Id,
                ProductId = product.Id > 0 ? (int?)product.Id : null,
                ProductSalePrice = buyNowModel.TotalPrice,
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                CategoryName = product.CategoryName,
                Quantity = 1,
                TotalPrice = buyNowModel.TotalPrice,
                ProductSpecItems = "",
                ProductImageUrl = product.CroppedImageUrl
            };

            var savedOrderProduct = OrderProductService.SaveOrEditEntity(entity);

            if (ProductService != null && product.Id > 0)
            {
                ProductService.DecreaseStock(product.Id, 1);
            }

            Logger.Debug($"BuyNow order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}, OrderProductId: {savedOrderProduct.Id}");
        }

        private async Task SaveOrderProductAsync(ShoppingCartSession shoppingCart, Order savedOrder)
        {
            await SaveOrderProductAsync(shoppingCart.ShoppingCartItems, savedOrder).ConfigureAwait(false);
        }

        private async Task SaveOrderProductAsync(List<ShoppingCartItem> shoppingCartItems, Order savedOrder)
        {
            Logger.Debug($"SaveOrderProductAsync started for OrderId: {savedOrder.Id}, ItemCount: {shoppingCartItems.Count}");

            foreach (var shoppingCartItem in shoppingCartItems)
            {
                var product = shoppingCartItem.Product;
                Logger.Debug($"Saving order product - OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}, Quantity: {shoppingCartItem.Quantity}");

                await OrderProductService.SaveOrEditEntityAsync(new OrderProduct()
                {
                    OrderId = savedOrder.Id,
                    ProductId = product.Id > 0 ? (int?)product.Id : null,
                    ProductSalePrice = product.Price,
                    ProductName = product.Name,
                    ProductCode = product.ProductCode,
                    CategoryName = product.CategoryName,
                    Quantity = shoppingCartItem.Quantity,
                    TotalPrice = shoppingCartItem.TotalPrice,
                    ProductSpecItems = JsonConvert.SerializeObject(product.ProductSpecItems),
                    ProductImageUrl = product.CroppedImageUrl
                }).ConfigureAwait(false);

                if (ProductService != null && product.Id > 0)
                {
                    await ProductService.DecreaseStockAsync(product.Id, shoppingCartItem.Quantity).ConfigureAwait(false);
                }

                Logger.Debug($"Order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}");
            }

            Logger.Debug($"All order products saved successfully for OrderId: {savedOrder.Id}");
        }

        private async Task SaveOrderProductAsync(BuyNowModel buyNowModel, Order savedOrder)
        {
            var product = buyNowModel.ShoppingCartItem.Product;
            Logger.Debug($"SaveOrderProductAsync (BuyNow) started for OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}");

            var entity = new OrderProduct()
            {
                OrderId = savedOrder.Id,
                ProductId = product.Id > 0 ? (int?)product.Id : null,
                ProductSalePrice = buyNowModel.TotalPrice,
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                CategoryName = product.CategoryName,
                Quantity = 1,
                TotalPrice = buyNowModel.TotalPrice,
                ProductSpecItems = "",
                ProductImageUrl = product.CroppedImageUrl
            };

            var savedOrderProduct = await OrderProductService.SaveOrEditEntityAsync(entity).ConfigureAwait(false);

            if (ProductService != null && product.Id > 0)
            {
                await ProductService.DecreaseStockAsync(product.Id, 1).ConfigureAwait(false);
            }

            Logger.Debug($"BuyNow order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}, OrderProductId: {savedOrderProduct.Id}");
        }

        [Timed("service.shopping_cart.get_admin_page_list_sync")]
        public virtual List<ShoppingCart> GetAdminPageList(string search, int currentLanguage)
        {
            return ShoppingCartRepository.GetAdminPageList(search, currentLanguage);
        }

        [Timed("service.shopping_cart.get_admin_page_list")]
        public virtual async Task<List<ShoppingCart>> GetAdminPageListAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ShoppingCartRepository.GetAdminPageListAsync(search, currentLanguage, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.shopping_cart.clear_expired_sync")]
        public virtual int ClearExpiredShoppingCarts(int olderThanDays = 30)
        {
            if (olderThanDays < 1)
            {
                olderThanDays = 30;
            }

            var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            Logger.Debug($"ClearExpiredShoppingCarts starting. CutoffDate: {cutoffDate:yyyy-MM-dd HH:mm:ss} (older than {olderThanDays} days)");
            int count = ShoppingCartRepository.DeleteExpiredShoppingCarts(cutoffDate);
            Logger.Info($"ClearExpiredShoppingCarts completed. Deleted {count} expired carts.");
            return count;
        }

        [Timed("service.shopping_cart.clear_expired")]
        public virtual async Task<int> ClearExpiredShoppingCartsAsync(int olderThanDays = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (olderThanDays < 1)
            {
                olderThanDays = 30;
            }

            var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            Logger.Debug($"ClearExpiredShoppingCartsAsync starting. CutoffDate: {cutoffDate:yyyy-MM-dd HH:mm:ss} (older than {olderThanDays} days)");
            int count = await ShoppingCartRepository.DeleteExpiredShoppingCartsAsync(cutoffDate, 500, cancellationToken).ConfigureAwait(false);
            Logger.Info($"ClearExpiredShoppingCartsAsync completed. Deleted {count} expired carts.");
            return count;
        }

        private CouponValidationResult ValidateCouponForOrderSync(ShoppingCartSession shoppingCart, string userId, string currency)
        {
            if (shoppingCart?.Coupon == null || string.IsNullOrWhiteSpace(shoppingCart.Coupon.Code)) return null;
            if (CouponValidationService == null) return null;
            try
            {
                var ctx = BuildCouponValidationContextSync(userId, shoppingCart, currency);
                var task = CouponValidationService.ValidateCouponAsync(shoppingCart.Coupon.Code, shoppingCart, ctx);
                // Use GetAwaiter().GetResult to avoid deadlock in sync path; ConfigureAwait false
                var result = task.ConfigureAwait(false).GetAwaiter().GetResult();
                if (!result.IsValid)
                {
                    Logger.Warn($"Coupon validation failed at order creation: {result.Reason} - {result.Message}");
                    throw new InvalidOperationException($"{result.Reason}: {result.Message}");
                }
                return result;
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                Logger.Error(ex, "Coupon validation sync failed");
                throw new InvalidOperationException("Coupon validation failed: " + ex.Message, ex);
            }
        }

        private async Task<CouponValidationResult> ValidateCouponForOrderAsync(ShoppingCartSession shoppingCart, string userId, string currency)
        {
            if (shoppingCart?.Coupon == null || string.IsNullOrWhiteSpace(shoppingCart.Coupon.Code)) return null;
            if (CouponValidationService == null) return null;
            var ctx = await BuildCouponValidationContextAsync(userId, shoppingCart, currency).ConfigureAwait(false);
            var result = await CouponValidationService.ValidateCouponAsync(shoppingCart.Coupon.Code, shoppingCart, ctx).ConfigureAwait(false);
            if (!result.IsValid)
            {
                Logger.Warn($"Coupon validation failed at order creation: {result.Reason} - {result.Message}");
                throw new InvalidOperationException($"{result.Reason}: {result.Message}");
            }
            return result;
        }

        private CouponValidationContext BuildCouponValidationContextSync(string userId, ShoppingCartSession shoppingCart, string currency)
        {
            var ctx = new CouponValidationContext
            {
                UserId = userId,
                IsAuthenticated = !string.IsNullOrEmpty(userId),
                Currency = currency,
                Language = shoppingCart.CurrentLanguage != 0 ? shoppingCart.CurrentLanguage : AppConfig.MainLanguage,
                CargoPrice = shoppingCart.CargoPriceValue,
                HasExistingCoupon = false
            };
            try
            {
                if (!string.IsNullOrEmpty(userId) && CustomerService != null)
                {
                    var cust = CustomerService.GetUserId(userId);
                    if (cust != null)
                    {
                        ctx.CustomerId = cust.Id;
                        ctx.BirthDate = cust.BirthDate;
                        ctx.CustomerCreatedDate = cust.CreatedDate;
                    }
                }
            }
            catch (Exception ex) { Logger.Warn(ex, "Failed to build coupon validation context sync"); }
            return ctx;
        }

        private async Task<CouponValidationContext> BuildCouponValidationContextAsync(string userId, ShoppingCartSession shoppingCart, string currency)
        {
            var ctx = new CouponValidationContext
            {
                UserId = userId,
                IsAuthenticated = !string.IsNullOrEmpty(userId),
                Currency = currency,
                Language = shoppingCart.CurrentLanguage != 0 ? shoppingCart.CurrentLanguage : AppConfig.MainLanguage,
                CargoPrice = shoppingCart.CargoPriceValue,
                HasExistingCoupon = false
            };
            try
            {
                if (!string.IsNullOrEmpty(userId) && CustomerService != null)
                {
                    var cust = await CustomerService.GetUserIdAsync(userId).ConfigureAwait(false);
                    if (cust != null)
                    {
                        ctx.CustomerId = cust.Id;
                        ctx.BirthDate = cust.BirthDate;
                        ctx.CustomerCreatedDate = cust.CreatedDate;
                    }
                }
            }
            catch (Exception ex) { Logger.Warn(ex, "Failed to build coupon validation context async"); }
            return ctx;
        }

        private void RecordCouponRedemptionSync(CouponValidationResult validation, Order order, string userId, ShoppingCartSession cart)
        {
            if (CouponRedemptionRepository == null) return;
            int? custId = null;
            try
            {
                if (!string.IsNullOrEmpty(userId) && CustomerService != null)
                {
                    var cust = CustomerService.GetUserId(userId);
                    custId = cust?.Id;
                }
            }
            catch { }
            var redemption = new CouponRedemption
            {
                Name = validation.CouponCode,
                CouponId = validation.CouponId.Value,
                OrderId = order.Id,
                CustomerId = custId,
                UserId = userId,
                CouponCode = validation.CouponCode,
                DiscountAmount = validation.DiscountAmount,
                OrderTotalBeforeDiscount = validation.EligibleAmount,
                Currency = order.Currency,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true,
                Position = 0,
                Lang = AppConfig.MainLanguage
            };
            CouponRedemptionRepository.SaveOrEdit(redemption);
        }

        private async Task RecordCouponRedemptionAsync(CouponValidationResult validation, Order order, string userId, ShoppingCartSession cart)
        {
            if (CouponRedemptionRepository == null) return;
            int? custId = null;
            try
            {
                if (!string.IsNullOrEmpty(userId) && CustomerService != null)
                {
                    var cust = await CustomerService.GetUserIdAsync(userId).ConfigureAwait(false);
                    custId = cust?.Id;
                }
            }
            catch { }
            var redemption = new CouponRedemption
            {
                Name = validation.CouponCode,
                CouponId = validation.CouponId.Value,
                OrderId = order.Id,
                CustomerId = custId,
                UserId = userId,
                CouponCode = validation.CouponCode,
                DiscountAmount = validation.DiscountAmount,
                OrderTotalBeforeDiscount = validation.EligibleAmount,
                Currency = order.Currency,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true,
                Position = 0,
                Lang = AppConfig.MainLanguage
            };
            await CouponRedemptionRepository.SaveOrEditAsync(redemption).ConfigureAwait(false);
        }
    }
}
