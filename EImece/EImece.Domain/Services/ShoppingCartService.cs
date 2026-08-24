using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Newtonsoft.Json;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ShoppingCartService : BaseEntityService<ShoppingCart>, IShoppingCartService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEImeceContext _dbContext;

        private IOrderService OrderService;

        private ICustomerService CustomerService;

        private IAddressService AddressService;

        private IOrderProductService OrderProductService;

        private IProductService ProductService;

        private IShoppingCartRepository ShoppingCartRepository { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        public ShoppingCartService(
            IEImeceContext dbContext,
            ApplicationUserManager userManager,
            IShoppingCartRepository repository,
            IOrderService orderService,
            ICustomerService customerService,
            IAddressService addressService,
            IOrderProductService orderProductService,
            IProductService productService = null) : base(repository)
        {
            Logger.Info("ShoppingCartService initialized");
            this._dbContext = dbContext;
            this.ShoppingCartRepository = repository;
            this.UserManager = userManager;
            this.OrderService = orderService;
            this.CustomerService = customerService;
            this.AddressService = addressService;
            this.OrderProductService = orderProductService;
            this.ProductService = productService;
        }

        public ShoppingCartService(
            ApplicationUserManager userManager,
            IShoppingCartRepository repository,
            IOrderService orderService,
            ICustomerService customerService,
            IAddressService addressService,
            IOrderProductService orderProductService,
            IProductService productService = null)
            : this(null, userManager, repository, orderService, customerService, addressService, orderProductService, productService)
        {
        }

        private EntitiesContext GetEntitiesContext()
        {
            if (_dbContext is EntitiesContext entitiesContext)
            {
                return entitiesContext;
            }

            if (ShoppingCartRepository is BaseRepository<ShoppingCart> baseRepo)
            {
                try
                {
                    return baseRepo.GetDbContext();
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }

            return null;
        }

        public void SaveOrEditShoppingCart(ShoppingCart item)
        {
            Logger.Info($"SaveOrEditShoppingCart called with OrderGuid: {item.OrderGuid}");
            var shoppingCart = ShoppingCartRepository.GetShoppingCartByOrderGuid(item.OrderGuid);
            if (shoppingCart == null)
            {
                Logger.Info($"No existing shopping cart found for OrderGuid: {item.OrderGuid}. Creating new cart.");
                shoppingCart = item;
            }
            else
            {
                Logger.Info($"Existing shopping cart found for OrderGuid: {item.OrderGuid}. Updating cart content.");
                shoppingCart.ShoppingCartJson = item.ShoppingCartJson;
            }
            ShoppingCartRepository.SaveOrEdit(shoppingCart);
            Logger.Info($"Shopping cart saved successfully for OrderGuid: {item.OrderGuid}");
        }

        public async Task SaveOrEditShoppingCartAsync(ShoppingCart item)
        {
            Logger.Info($"SaveOrEditShoppingCartAsync called with OrderGuid: {item.OrderGuid}");
            var shoppingCart = await ShoppingCartRepository.GetShoppingCartByOrderGuidAsync(item.OrderGuid).ConfigureAwait(false);
            if (shoppingCart == null)
            {
                Logger.Info($"No existing shopping cart found for OrderGuid: {item.OrderGuid}. Creating new cart.");
                shoppingCart = item;
            }
            else
            {
                Logger.Info($"Existing shopping cart found for OrderGuid: {item.OrderGuid}. Updating cart content.");
                shoppingCart.ShoppingCartJson = item.ShoppingCartJson;
            }
            await ShoppingCartRepository.SaveOrEditAsync(shoppingCart).ConfigureAwait(false);
            Logger.Info($"Shopping cart saved successfully for OrderGuid: {item.OrderGuid}");
        }

        public ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            Logger.Info($"GetShoppingCartByOrderGuid called with OrderGuid: {orderGuid}");
            var cart = ShoppingCartRepository.GetShoppingCartByOrderGuid(orderGuid);
            Logger.Info($"GetShoppingCartByOrderGuid result: {(cart == null ? "No cart found" : "Cart found")}");
            return cart;
        }

        public async Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid)
        {
            Logger.Info($"GetShoppingCartByOrderGuidAsync called with OrderGuid: {orderGuid}");
            var cart = await ShoppingCartRepository.GetShoppingCartByOrderGuidAsync(orderGuid).ConfigureAwait(false);
            Logger.Info($"GetShoppingCartByOrderGuidAsync result: {(cart == null ? "No cart found" : "Cart found")}");
            return cart;
        }

        public void DeleteByOrderGuid(string orderGuid)
        {
            Logger.Info($"DeleteByOrderGuid called with OrderGuid: {orderGuid}");
            ShoppingCartRepository.DeleteByWhereCondition(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase));
            Logger.Info($"Shopping cart deleted for OrderGuid: {orderGuid}");
        }

        public async Task DeleteByOrderGuidAsync(string orderGuid)
        {
            Logger.Info($"DeleteByOrderGuidAsync called with OrderGuid: {orderGuid}");
            var carts = await ShoppingCartRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).ToListAsync().ConfigureAwait(false);
            foreach (var cart in carts)
            {
                ShoppingCartRepository.Delete(cart);
            }
            if (carts.Count > 0)
            {
                await ShoppingCartRepository.SaveAsync().ConfigureAwait(false);
            }
            Logger.Info($"Shopping cart deleted for OrderGuid: {orderGuid}");
        }

        public Order SaveShoppingCart(string orderNumber, ShoppingCartSession shoppingCart, PaymentResult paymentResult, string userId)
        {
            Logger.Info($"SaveShoppingCart started - UserId: {userId}, OrderGuid: {shoppingCart?.OrderGuid}");

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

            var dbContext = GetEntitiesContext();
            using (var transaction = dbContext?.Database?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    Logger.Info($"Processing addresses - Initial ShippingAddressId: {shoppingCart.ShippingAddress.Id}, BillingAddressId: {shoppingCart.BillingAddress.Id}");

                    int shippingAddressId = shoppingCart.ShippingAddress.Id;
                    int billingAddressId = shoppingCart.BillingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Info("Creating new shipping address");
                        shoppingCart.ShippingAddress.Name = Resource.ShippingAddress;
                        shoppingCart.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shoppingCart.ShippingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                        shoppingCart.ShippingAddress.City = shoppingCart.Customer.City.ToStr();
                        shoppingCart.ShippingAddress.Country = shoppingCart.Customer.Country.ToStr();
                        shoppingCart.ShippingAddress.ZipCode = shoppingCart.Customer.ZipCode.ToStr();
                        var shippingAddress = AddressService.SaveOrEditEntity(shoppingCart.ShippingAddress.ToEntity());
                        shippingAddressId = shippingAddress.Id;
                        Logger.Info($"New shipping address created with Id: {shippingAddressId}");
                    }
                    if (billingAddressId == 0)
                    {
                        Logger.Info("Creating new billing address");
                        shoppingCart.BillingAddress.Name = Resource.BillingAdress;
                        shoppingCart.BillingAddress.AddressType = (int)AddressType.BillingAddress;
                        shoppingCart.BillingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                        shoppingCart.BillingAddress.City = shoppingCart.Customer.City.ToStr();
                        shoppingCart.BillingAddress.Country = shoppingCart.Customer.Country.ToStr();
                        shoppingCart.BillingAddress.ZipCode = shoppingCart.Customer.ZipCode.ToStr();
                        var billingAddress = AddressService.SaveOrEditEntity(shoppingCart.BillingAddress.ToEntity());
                        billingAddressId = billingAddress.Id;
                        Logger.Info($"New billing address created with Id: {billingAddressId}");
                    }

                    Logger.Info($"Saving customer type to normal for userId: {userId}");
                    CustomerService.SaveCustomerTypeToNormal(userId);

                    Logger.Info($"Creating order for userId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");
                    Order savedOrder = SaveOrder(orderNumber, userId, shoppingCart, paymentResult, shippingAddressId, billingAddressId);
                    Logger.Info($"Order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Info($"Saving order products for OrderId: {savedOrder.Id}");
                    SaveOrderProduct(shoppingCart, savedOrder);
                    Logger.Info($"Order products saved successfully for OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Info($"SaveShoppingCart completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
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

        public async Task<Order> SaveShoppingCartAsync(string orderNumber, ShoppingCartSession shoppingCart, PaymentResult paymentResult, string userId)
        {
            Logger.Info($"SaveShoppingCartAsync started - UserId: {userId}, OrderGuid: {shoppingCart?.OrderGuid}");

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

            var dbContext = GetEntitiesContext();
            using (var transaction = dbContext?.Database?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
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

                    Logger.Debug($"Creating order for userId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");
                    Order savedOrder = await SaveOrderAsync(orderNumber, userId, shoppingCart, paymentResult, shippingAddressId, billingAddressId).ConfigureAwait(false);
                    Logger.Debug($"Order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    await SaveOrderProductAsync(shoppingCart, savedOrder).ConfigureAwait(false);
                    Logger.Debug($"Order products saved successfully for OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Info($"SaveShoppingCartAsync completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
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
           int billingAddressId)
        {
            Logger.Info($"SaveOrder started - UserId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");

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
            item.CargoPrice = shoppingCart.CargoPriceValue;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = shoppingCart.Coupon != null ? shoppingCart.Coupon.Name : "";
            item.CouponDiscount = shoppingCart.CalculateCouponDiscount(shoppingCart.TotalPrice).CurrencySignForIyizo();
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

            Logger.Info($"Saving order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            Logger.Info($"Order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private Order SaveOrder(String orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult,
          int shippingAddressId)
        {
            Logger.Info($"SaveOrder (buyWithNoAccountCreation) started - UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");

            var item = new Order();

            item.OrderComments = buyWithNoAccountCreation.OrderComments;
            item.Name = buyWithNoAccountCreation.Customer.FullName;
            item.OrderGuid = buyWithNoAccountCreation.OrderGuid;
            item.OrderType = (int)EImeceOrderType.BuyWithNoAccountCreation;
            item.OrderNumber = orderNumber;
            item.CargoPrice = buyWithNoAccountCreation.CargoPriceValue;
            item.UserId = buyWithNoAccountCreation.Customer.UserId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = buyWithNoAccountCreation.CouponStr;
            item.CouponDiscount = buyWithNoAccountCreation.CalculateCouponDiscount(buyWithNoAccountCreation.TotalPrice).CurrencySignForIyizo();
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

            Logger.Info($"Saving buyWithNoAccountCreation order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            Logger.Info($"buyWithNoAccountCreation order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private Order SaveOrder(String userId, BuyNowModel buyNowSession, PaymentResult paymentResult,
          int shippingAddressId)
        {
            Logger.Info($"SaveOrder (BuyNow) started - UserId: {userId}, ShippingAddressId: {shippingAddressId}");

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

            Logger.Info($"Saving BuyNow order with OrderNumber: {item.OrderNumber}, OrderGuid: {item.OrderGuid}, PaymentId: {item.PaymentId}");
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            Logger.Info($"BuyNow order saved successfully with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

            return savedOrder;
        }

        private async Task<Order> SaveOrderAsync(string orderNumber, String userId, ShoppingCartSession shoppingCart, PaymentResult paymentResult,
            int shippingAddressId,
           int billingAddressId)
        {
            Logger.Info($"SaveOrderAsync started - UserId: {userId}, ShippingAddressId: {shippingAddressId}, BillingAddressId: {billingAddressId}");

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
            item.CargoPrice = shoppingCart.CargoPriceValue;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = shoppingCart.Coupon != null ? shoppingCart.Coupon.Name : "";
            item.CouponDiscount = shoppingCart.CalculateCouponDiscount(shoppingCart.TotalPrice).CurrencySignForIyizo();
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
          int shippingAddressId)
        {
            Logger.Info($"SaveOrderAsync (buyWithNoAccountCreation) started - UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");

            var item = new Order();

            item.OrderComments = buyWithNoAccountCreation.OrderComments;
            item.Name = buyWithNoAccountCreation.Customer.FullName;
            item.OrderGuid = buyWithNoAccountCreation.OrderGuid;
            item.OrderType = (int)EImeceOrderType.BuyWithNoAccountCreation;
            item.OrderNumber = orderNumber;
            item.CargoPrice = buyWithNoAccountCreation.CargoPriceValue;
            item.UserId = buyWithNoAccountCreation.Customer.UserId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = AppConfig.MainLanguage;
            item.Coupon = buyWithNoAccountCreation.CouponStr;
            item.CouponDiscount = buyWithNoAccountCreation.CalculateCouponDiscount(buyWithNoAccountCreation.TotalPrice).CurrencySignForIyizo();
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
            Logger.Info($"SaveOrderAsync (BuyNow) started - UserId: {userId}, ShippingAddressId: {shippingAddressId}");

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

        public Order SaveBuyWithNoAccountCreation(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult)
        {
            Logger.Info($"SaveBuyWithNoAccountCreation started - OrderGuid: {buyWithNoAccountCreation?.OrderGuid}");

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

            var dbContext = GetEntitiesContext();
            using (var transaction = dbContext?.Database?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    CustomerDto customer = buyWithNoAccountCreation.Customer;
                    AddressDto shippingAddress = buyWithNoAccountCreation.ShippingAddress;
                    int shippingAddressId = shippingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Info("Creating new shipping address for BuyWithNoAccountCreation order");
                        shippingAddress.Name = Resource.ShippingAddress;
                        shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shippingAddress.Description = customer.RegistrationAddress;
                        shippingAddress.City = customer.City;
                        shippingAddress.Country = customer.Country;
                        shippingAddress.ZipCode = customer.ZipCode;
                        var savedShippingAddress = AddressService.SaveOrEditEntity(shippingAddress.ToEntity());
                        shippingAddressId = savedShippingAddress.Id;
                        Logger.Info($"New shipping address created with Id: {shippingAddressId}");
                    }

                    Logger.Info($"Creating buyWithNoAccountCreation order for UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = SaveOrder(orderNumber, buyWithNoAccountCreation, paymentResult, shippingAddressId);
                    Logger.Info($"buyWithNoAccountCreation order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Info($"Saving order product for buyWithNoAccountCreation OrderId: {savedOrder.Id}");
                    SaveOrderProduct(buyWithNoAccountCreation.ShoppingCartItems, savedOrder);
                    Logger.Info($"Order product saved successfully for buyWithNoAccountCreation OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Info($"SaveBuyWithNoAccountCreation completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
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

        public Order SaveBuyNow(BuyNowModel buyNowSession, PaymentResult paymentResult)
        {
            Logger.Info($"SaveBuyNow started - OrderGuid: {buyNowSession?.OrderGuid}");

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

            var dbContext = GetEntitiesContext();
            using (var transaction = dbContext?.Database?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    Logger.Info("Saving customer information");
                    CustomerDto customer = buyNowSession.Customer;
                    customer.CustomerType = (int)EImeceCustomerType.BuyNow;
                    customer.CreatedDate = DateTime.Now;
                    customer.UpdatedDate = DateTime.Now;
                    var customerEntity = CustomerService.SaveOrEditEntity(customer.ToEntity());
                    Logger.Info($"Customer saved with Id: {customerEntity.Id}");

                    buyNowSession.Customer.UserId = GeneralHelper.RandomNumber(12) + "-" + Constants.BuyNowCustomerUserId + "-" + customerEntity.Id;
                    Logger.Info($"Generated UserId for BuyNow customer: {buyNowSession.Customer.UserId}");

                    AddressDto shippingAddress = buyNowSession.ShippingAddress;
                    int shippingAddressId = shippingAddress.Id;
                    if (shippingAddressId == 0)
                    {
                        Logger.Info("Creating new shipping address for BuyNow order");
                        shippingAddress.Name = Resource.ShippingAddress;
                        shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                        shippingAddress.Description = customer.RegistrationAddress;
                        shippingAddress.City = customer.City;
                        shippingAddress.Country = customer.Country;
                        shippingAddress.ZipCode = customer.ZipCode;
                        var savedShippingAddress = AddressService.SaveOrEditEntity(shippingAddress.ToEntity());
                        shippingAddressId = savedShippingAddress.Id;
                        Logger.Info($"New shipping address created with Id: {shippingAddressId}");
                    }

                    Logger.Info($"Creating BuyNow order for UserId: {buyNowSession.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = SaveOrder(buyNowSession.Customer.UserId, buyNowSession, paymentResult, shippingAddressId);
                    Logger.Info($"BuyNow order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Info($"Saving order product for BuyNow OrderId: {savedOrder.Id}");
                    SaveOrderProduct(buyNowSession, savedOrder);
                    Logger.Info($"Order product saved successfully for BuyNow OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Info($"SaveBuyNow completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
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

        public async Task<Order> SaveBuyWithNoAccountCreationAsync(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult)
        {
            Logger.Info($"SaveBuyWithNoAccountCreationAsync started - OrderGuid: {buyWithNoAccountCreation?.OrderGuid}");

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

            var dbContext = GetEntitiesContext();
            using (var transaction = dbContext?.Database?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
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

                    Logger.Debug($"Creating buyWithNoAccountCreation order for UserId: {buyWithNoAccountCreation.Customer.UserId}, ShippingAddressId: {shippingAddressId}");
                    Order savedOrder = await SaveOrderAsync(orderNumber, buyWithNoAccountCreation, paymentResult, shippingAddressId).ConfigureAwait(false);
                    Logger.Debug($"buyWithNoAccountCreation order created with Id: {savedOrder.Id}, OrderNumber: {savedOrder.OrderNumber}");

                    Logger.Debug($"Saving order product for buyWithNoAccountCreation OrderId: {savedOrder.Id}");
                    await SaveOrderProductAsync(buyWithNoAccountCreation.ShoppingCartItems, savedOrder).ConfigureAwait(false);
                    Logger.Debug($"Order product saved successfully for buyWithNoAccountCreation OrderId: {savedOrder.Id}");

                    transaction?.Commit();

                    Logger.Info($"SaveBuyWithNoAccountCreationAsync completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
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

        public async Task<Order> SaveBuyNowAsync(BuyNowModel buyNowSession, PaymentResult paymentResult)
        {
            Logger.Info($"SaveBuyNowAsync started - OrderGuid: {buyNowSession?.OrderGuid}");

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

            var dbContext = GetEntitiesContext();
            using (var transaction = dbContext?.Database?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
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

                    Logger.Info($"SaveBuyNowAsync completed successfully for OrderId: {savedOrder.Id}, OrderGuid: {savedOrder.OrderGuid}");
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
            Logger.Info($"SaveOrderProduct started for OrderId: {savedOrder.Id}, ItemCount: {shoppingCartItems.Count}");

            foreach (var shoppingCartItem in shoppingCartItems)
            {
                var product = shoppingCartItem.Product;
                Logger.Info($"Saving order product - OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}, Quantity: {shoppingCartItem.Quantity}");

                OrderProductService.SaveOrEditEntity(new OrderProduct()
                {
                    OrderId = savedOrder.Id,
                    ProductId = product.Id,
                    ProductSalePrice = product.Price,
                    ProductName = product.Name,
                    ProductCode = product.ProductCode,
                    CategoryName = product.CategoryName,
                    Quantity = shoppingCartItem.Quantity,
                    TotalPrice = shoppingCartItem.TotalPrice,
                    ProductSpecItems = JsonConvert.SerializeObject(product.ProductSpecItems)
                });

                if (ProductService != null && product.Id > 0)
                {
                    ProductService.DecreaseStock(product.Id, shoppingCartItem.Quantity);
                }

                Logger.Info($"Order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}");
            }

            Logger.Info($"All order products saved successfully for OrderId: {savedOrder.Id}");
        }

        private void SaveOrderProduct(BuyNowModel buyNowModel, Order savedOrder)
        {
            var product = buyNowModel.ShoppingCartItem.Product;
            Logger.Info($"SaveOrderProduct (BuyNow) started for OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}");

            var entity = new OrderProduct()
            {
                OrderId = savedOrder.Id,
                ProductId = product.Id,
                ProductSalePrice = buyNowModel.TotalPrice,
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                CategoryName = product.CategoryName,
                Quantity = 1,
                TotalPrice = buyNowModel.TotalPrice,
                ProductSpecItems = ""
            };

            var savedOrderProduct = OrderProductService.SaveOrEditEntity(entity);

            if (ProductService != null && product.Id > 0)
            {
                ProductService.DecreaseStock(product.Id, 1);
            }

            Logger.Info($"BuyNow order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}, OrderProductId: {savedOrderProduct.Id}");
        }

        private async Task SaveOrderProductAsync(ShoppingCartSession shoppingCart, Order savedOrder)
        {
            await SaveOrderProductAsync(shoppingCart.ShoppingCartItems, savedOrder).ConfigureAwait(false);
        }

        private async Task SaveOrderProductAsync(List<ShoppingCartItem> shoppingCartItems, Order savedOrder)
        {
            Logger.Info($"SaveOrderProductAsync started for OrderId: {savedOrder.Id}, ItemCount: {shoppingCartItems.Count}");

            foreach (var shoppingCartItem in shoppingCartItems)
            {
                var product = shoppingCartItem.Product;
                Logger.Info($"Saving order product - OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}, Quantity: {shoppingCartItem.Quantity}");

                await OrderProductService.SaveOrEditEntityAsync(new OrderProduct()
                {
                    OrderId = savedOrder.Id,
                    ProductId = product.Id,
                    ProductSalePrice = product.Price,
                    ProductName = product.Name,
                    ProductCode = product.ProductCode,
                    CategoryName = product.CategoryName,
                    Quantity = shoppingCartItem.Quantity,
                    TotalPrice = shoppingCartItem.TotalPrice,
                    ProductSpecItems = JsonConvert.SerializeObject(product.ProductSpecItems)
                }).ConfigureAwait(false);

                if (ProductService != null && product.Id > 0)
                {
                    await ProductService.DecreaseStockAsync(product.Id, shoppingCartItem.Quantity).ConfigureAwait(false);
                }

                Logger.Info($"Order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}");
            }

            Logger.Info($"All order products saved successfully for OrderId: {savedOrder.Id}");
        }

        private async Task SaveOrderProductAsync(BuyNowModel buyNowModel, Order savedOrder)
        {
            var product = buyNowModel.ShoppingCartItem.Product;
            Logger.Info($"SaveOrderProductAsync (BuyNow) started for OrderId: {savedOrder.Id}, ProductId: {product.Id}, ProductName: {product.Name}");

            var entity = new OrderProduct()
            {
                OrderId = savedOrder.Id,
                ProductId = product.Id,
                ProductSalePrice = buyNowModel.TotalPrice,
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                CategoryName = product.CategoryName,
                Quantity = 1,
                TotalPrice = buyNowModel.TotalPrice,
                ProductSpecItems = ""
            };

            var savedOrderProduct = await OrderProductService.SaveOrEditEntityAsync(entity).ConfigureAwait(false);

            if (ProductService != null && product.Id > 0)
            {
                await ProductService.DecreaseStockAsync(product.Id, 1).ConfigureAwait(false);
            }

            Logger.Info($"BuyNow order product saved successfully - OrderId: {savedOrder.Id}, ProductId: {product.Id}, OrderProductId: {savedOrderProduct.Id}");
        }

        public List<ShoppingCart> GetAdminPageList(string search, int currentLanguage)
        {
            return ShoppingCartRepository.GetAdminPageList(search, currentLanguage);
        }

        public async Task<List<ShoppingCart>> GetAdminPageListAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ShoppingCartRepository.GetAdminPageListAsync(search, currentLanguage, cancellationToken).ConfigureAwait(false);
        }

        public int ClearExpiredShoppingCarts(int olderThanDays = 30)
        {
            if (olderThanDays < 1)
            {
                olderThanDays = 30;
            }

            var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            Logger.Info($"ClearExpiredShoppingCarts starting. CutoffDate: {cutoffDate:yyyy-MM-dd HH:mm:ss} (older than {olderThanDays} days)");
            int count = ShoppingCartRepository.DeleteExpiredShoppingCarts(cutoffDate);
            Logger.Info($"ClearExpiredShoppingCarts completed. Deleted {count} expired carts.");
            return count;
        }

        public async Task<int> ClearExpiredShoppingCartsAsync(int olderThanDays = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (olderThanDays < 1)
            {
                olderThanDays = 30;
            }

            var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            Logger.Info($"ClearExpiredShoppingCartsAsync starting. CutoffDate: {cutoffDate:yyyy-MM-dd HH:mm:ss} (older than {olderThanDays} days)");
            int count = await ShoppingCartRepository.DeleteExpiredShoppingCartsAsync(cutoffDate, 500, cancellationToken).ConfigureAwait(false);
            Logger.Info($"ClearExpiredShoppingCartsAsync completed. Deleted {count} expired carts.");
            return count;
        }
    }
}
