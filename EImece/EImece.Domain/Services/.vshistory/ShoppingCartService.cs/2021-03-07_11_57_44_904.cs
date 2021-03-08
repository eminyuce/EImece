using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Iyzipay.Model;
using Newtonsoft.Json;
using NLog;
using Resources;
using System;

namespace EImece.Domain.Services
{
    public class ShoppingCartService : BaseEntityService<ShoppingCart>, IShoppingCartService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IOrderService OrderService;

        private ICustomerService CustomerService;

        private IAddressService AddressService;

        private IOrderProductService OrderProductService;

        private IShoppingCartRepository ShoppingCartRepository { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        public ShoppingCartService(ApplicationUserManager userManager,
            IShoppingCartRepository repository,
            IOrderService orderService,
            ICustomerService customerService,
            IAddressService addressService,
            IOrderProductService orderProductService) : base(repository)
        {
            ShoppingCartRepository = repository;
            this.UserManager = userManager;
            this.OrderService = orderService;
            this.CustomerService = customerService;
            this.AddressService = addressService;
            this.OrderProductService = orderProductService;
        }

        public void SaveOrEditShoppingCart(ShoppingCart item)
        {
            var shoppingCart = ShoppingCartRepository.GetShoppingCartByOrderGuid(item.OrderGuid);
            if (shoppingCart == null)
            {
                shoppingCart = item;
            }
            else
            {
                shoppingCart.ShoppingCartJson = item.ShoppingCartJson;
            }
            ShoppingCartRepository.SaveOrEdit(shoppingCart);
        }

        public ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            return ShoppingCartRepository.GetShoppingCartByOrderGuid(orderGuid);
        }

        public void DeleteByOrderGuid(string orderGuid)
        {
            ShoppingCartRepository.DeleteByWhereCondition(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase));
        }

        public Order SaveShoppingCart(ShoppingCartSession shoppingCart, CheckoutForm checkoutForm, string userId)
        {
            if (shoppingCart == null)
            {
                throw new ArgumentNullException("ShoppingCartSession", "ShoppingCartSession is null");
            }
            if (checkoutForm == null)
            {
                throw new ArgumentNullException("CheckoutForm", "CheckoutForm is null");
            }

            Entities.Address shippingAddress = shoppingCart.ShippingAddress;
            int shippingAddressId = shippingAddress.Id;
            Entities.Address billingAddress = shoppingCart.BillingAddress;
            int billingAddressId = billingAddress.Id;
            if (shippingAddressId == 0)
            {
                shippingAddress.Name = Resource.ShippingAddress;
                shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shippingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                shippingAddress.City = shoppingCart.Customer.City;
                shippingAddress.Country = shoppingCart.Customer.Country;
                shippingAddress.ZipCode = shoppingCart.Customer.ZipCode;
                shippingAddress = AddressService.SaveOrEditEntity(shoppingCart.ShippingAddress);
                shippingAddressId = shippingAddress.Id;
            }
            if (billingAddressId == 0)
            {
                billingAddress.Name = Resource.BillingAdress;
                billingAddress.AddressType = (int)AddressType.BillingAddress;
                billingAddress.Description = shoppingCart.Customer.RegistrationAddress;
                billingAddress.City = shoppingCart.Customer.City;
                billingAddress.Country = shoppingCart.Customer.Country;
                billingAddress.ZipCode = shoppingCart.Customer.ZipCode;
                billingAddress = AddressService.SaveOrEditEntity(billingAddress);
                billingAddressId = billingAddress.Id;
            }

            CustomerService.SaveShippingAddress(userId);
            Order savedOrder = SaveOrder(userId, shoppingCart, checkoutForm, shippingAddressId, billingAddressId);
            SaveOrderProduct(shoppingCart, savedOrder);

            return savedOrder;
        }

        private Order SaveOrder(String userId, ShoppingCartSession shoppingCart, CheckoutForm checkoutForm,
            int shippingAddressId,
           int billingAddressId)
        {
            var item = new Order();
            item.OrderComments = shoppingCart.OrderComments;
            item.Name = shoppingCart.Customer.FullName;
            item.OrderGuid = shoppingCart.OrderGuid;

            item.OrderNumber = GeneralHelper.RandomNumber(12);
            item.CargoPrice = shoppingCart.CargoPriceValue;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = 1;
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = billingAddressId;
            item.Coupon = "";
            item.Token = checkoutForm.Token;
            item.Price = checkoutForm.Price;
            item.PaidPrice = checkoutForm.PaidPrice;
            item.Installment = checkoutForm.PaidPrice;
            item.Currency = checkoutForm.PaidPrice;
            item.PaymentId = checkoutForm.PaymentId;
            item.PaymentStatus = checkoutForm.PaymentStatus;
            item.FraudStatus = checkoutForm.FraudStatus;
            item.MerchantCommissionRate = checkoutForm.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = checkoutForm.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = checkoutForm.IyziCommissionRateAmount;
            item.IyziCommissionFee = checkoutForm.IyziCommissionFee;
            item.CardType = checkoutForm.CardType;
            item.CardAssociation = checkoutForm.CardAssociation;
            item.CardFamily = checkoutForm.CardFamily;
            item.CardToken = checkoutForm.CardToken;
            item.CardUserKey = checkoutForm.CardUserKey;
            item.BinNumber = checkoutForm.BinNumber;
            item.LastFourDigits = checkoutForm.LastFourDigits;
            item.BasketId = checkoutForm.BasketId;
            item.ConversationId = checkoutForm.ConversationId;
            item.ConnectorName = checkoutForm.ConnectorName;
            item.AuthCode = checkoutForm.AuthCode;
            item.HostReference = checkoutForm.HostReference;
            item.Phase = checkoutForm.Phase;
            item.Status = checkoutForm.Status;
            item.ErrorCode = checkoutForm.ErrorCode;
            item.ErrorMessage = checkoutForm.ErrorMessage;
            item.Locale = checkoutForm.Locale;

            item.SystemTime = checkoutForm.SystemTime;
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            return savedOrder;
        }

        public Order SaveBuyNow(BuyNowModel buyNowSession, CheckoutForm checkoutForm)
        {
            buyNowSession.Customer.UserId = "BuyNowCustomer";
            Customer customer = buyNowSession.Customer;
            customer = CustomerService.SaveOrEditEntity(customer);
            Entities.Address shippingAddress = buyNowSession.ShippingAddress;
            int shippingAddressId = shippingAddress.Id;
            if (shippingAddressId == 0)
            {
                shippingAddress.Name = Resource.ShippingAddress;
                shippingAddress.AddressType = (int)AddressType.ShippingAddress;
                shippingAddress.Description = customer.RegistrationAddress;
                shippingAddress.City = customer.City;
                shippingAddress.Country = customer.Country;
                shippingAddress.ZipCode = customer.ZipCode;
                shippingAddress = AddressService.SaveOrEditEntity(buyNowSession.ShippingAddress);
                shippingAddressId = shippingAddress.Id;
            }
            Order savedOrder = SaveOrder(buyNowSession.Customer.UserId+"-" +buyNowSession.Customer.Id, buyNowSession, checkoutForm, shippingAddressId, shippingAddressId);
        
        }
        private Order SaveOrder(String userId, BuyNowModel buyNowSession, CheckoutForm checkoutForm,
          int shippingAddressId,
         int billingAddressId)
        {
            var item = new Order();
            item.OrderComments = "";
            item.Name = buyNowSession.Customer.FullName;
            item.OrderGuid = buyNowSession.OrderGuid;

            item.OrderNumber = GeneralHelper.RandomNumber(12);
            item.CargoPrice = shoppingCart.CargoPriceValue;
            item.UserId = userId;
            item.OrderStatus = (int)EImeceOrderStatus.NewlyOrder;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = 1;
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = billingAddressId;
            item.Coupon = "";
            item.Token = checkoutForm.Token;
            item.Price = checkoutForm.Price;
            item.PaidPrice = checkoutForm.PaidPrice;
            item.Installment = checkoutForm.PaidPrice;
            item.Currency = checkoutForm.PaidPrice;
            item.PaymentId = checkoutForm.PaymentId;
            item.PaymentStatus = checkoutForm.PaymentStatus;
            item.FraudStatus = checkoutForm.FraudStatus;
            item.MerchantCommissionRate = checkoutForm.MerchantCommissionRate;
            item.MerchantCommissionRateAmount = checkoutForm.MerchantCommissionRateAmount;
            item.IyziCommissionRateAmount = checkoutForm.IyziCommissionRateAmount;
            item.IyziCommissionFee = checkoutForm.IyziCommissionFee;
            item.CardType = checkoutForm.CardType;
            item.CardAssociation = checkoutForm.CardAssociation;
            item.CardFamily = checkoutForm.CardFamily;
            item.CardToken = checkoutForm.CardToken;
            item.CardUserKey = checkoutForm.CardUserKey;
            item.BinNumber = checkoutForm.BinNumber;
            item.LastFourDigits = checkoutForm.LastFourDigits;
            item.BasketId = checkoutForm.BasketId;
            item.ConversationId = checkoutForm.ConversationId;
            item.ConnectorName = checkoutForm.ConnectorName;
            item.AuthCode = checkoutForm.AuthCode;
            item.HostReference = checkoutForm.HostReference;
            item.Phase = checkoutForm.Phase;
            item.Status = checkoutForm.Status;
            item.ErrorCode = checkoutForm.ErrorCode;
            item.ErrorMessage = checkoutForm.ErrorMessage;
            item.Locale = checkoutForm.Locale;

            item.SystemTime = checkoutForm.SystemTime;
            Order savedOrder = OrderService.SaveOrEditEntity(item);
            return savedOrder;
        }

        private void SaveOrderProduct(ShoppingCartSession shoppingCart, Order savedOrder)
        {
            foreach (var shoppingCartItem in shoppingCart.ShoppingCartItems)
            {
                OrderProductService.SaveOrEditEntity(new OrderProduct()
                {
                    OrderId = savedOrder.Id,
                    ProductId = shoppingCartItem.Product.Id,
                    ProductSalePrice = shoppingCartItem.Product.Price,
                    ProductName = shoppingCartItem.Product.Name,
                    ProductCode = shoppingCartItem.Product.ProductCode,
                    CategoryName = shoppingCartItem.Product.CategoryName,
                    Quantity = shoppingCartItem.Quantity,
                    TotalPrice = shoppingCartItem.TotalPrice,
                    ProductSpecItems = JsonConvert.SerializeObject(shoppingCartItem.Product.ProductSpecItems)
                });  
            }
        }
    }
}