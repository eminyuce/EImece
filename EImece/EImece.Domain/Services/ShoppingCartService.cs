using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Iyzipay.Model;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Web;

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

        public Order SaveShoppingCart(ShoppingCartSession shoppingCart, CheckoutForm checkoutForm)
        {
            var userName = HttpContext.Current.User.Identity.GetUserName();
            var user = UserManager.FindByName(userName);
            if (user == null)
            {
                throw new ArgumentException("User cannot be null " + userName);
            }
            int shippingAddressId = shoppingCart.ShippingAddress.Id;
            int billingAddressId = shoppingCart.BillingAddress.Id;
            if (shippingAddressId == 0)
            {
                var shippingAddress = AddressService.SaveOrEditEntity(shoppingCart.ShippingAddress);
                shippingAddressId = shippingAddress.Id;
            }
            if (billingAddressId == 0)
            {
                var billingAddress = AddressService.SaveOrEditEntity(shoppingCart.BillingAddress);
                billingAddressId = billingAddress.Id;
            }
            CustomerService.SaveShippingAddress(user.Id);
            Order savedOrder = SaveOrder(user.Id, shoppingCart, checkoutForm, shippingAddressId, billingAddressId);
            SaveOrderProduct(shoppingCart, savedOrder);

            return savedOrder;
        }

        private Order SaveOrder(String userId, ShoppingCartSession shoppingCart, CheckoutForm checkoutForm,
            int shippingAddressId,
           int billingAddressId)
        {
            var item = new Order();
            item.OrderComments = shoppingCart.OrderComments;
            item.OrderNumber = GeneralHelper.RandomNumber(12);
            item.UserId = userId;
            item.Name = shoppingCart.Customer.FullName;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.IsActive = true;
            item.Position = 1;
            item.Lang = 1;
            item.DeliveryDate = DateTime.Now;
            item.ShippingAddressId = shippingAddressId;
            item.BillingAddressId = billingAddressId;
            item.OrderGuid = shoppingCart.OrderGuid;
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
                    Quantity = shoppingCartItem.Quantity,
                    TotalPrice = shoppingCartItem.TotalPrice
                });
            }
        }
    }
}