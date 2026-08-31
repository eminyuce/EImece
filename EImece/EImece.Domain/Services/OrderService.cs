using Microsoft.Extensions.Logging;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class OrderService : BaseEntityService<Order>, IOrderService
    {
        private readonly IOrderRepository OrderRepository;
        private readonly IOrderProductService OrderProductService;
        private readonly ICustomerService CustomerService;
        private readonly IAddressService AddressService;

        public OrderService(IOrderRepository repository,
            ILogger<OrderService> logger,
            ICustomerService customerService = null,
            IOrderProductService orderProductService = null,
            IAddressService addressService = null) : base(repository, logger) {
            OrderRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            CustomerService = customerService;
            OrderProductService = orderProductService;
            AddressService = addressService;
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        [Timed("service.orders.get_by_id", "Time taken to get order by id")]
        public virtual async Task<OrderDto> GetStorefrontOrderByIdAsync(int orderId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderByIdAsync(orderId, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.orders.get_by_id_sync")]
        public virtual OrderDto GetStorefrontOrderById(int orderId)
        {
            return OrderRepository.GetStorefrontOrderById(orderId);
        }

        public async Task<OrderDto> GetStorefrontOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderByOrderNumberAsync(orderNumber, cancellationToken).ConfigureAwait(false);
        }

        public OrderDto GetStorefrontOrderByOrderNumber(string orderNumber)
        {
            return OrderRepository.GetStorefrontOrderByOrderNumber(orderNumber);
        }

        public async Task<OrderDto> GetStorefrontOrderByGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderByGuidAsync(orderGuid, cancellationToken).ConfigureAwait(false);
        }

        public OrderDto GetStorefrontOrderByGuid(string orderGuid)
        {
            return OrderRepository.GetStorefrontOrderByGuid(orderGuid);
        }

        public async Task<List<OrderDto>> GetStorefrontOrdersByUserIdAsync(string userId, string search = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrdersByUserIdAsync(userId, search, cancellationToken).ConfigureAwait(false);
        }

        public List<OrderDto> GetStorefrontOrdersByUserId(string userId, string search = "")
        {
            return OrderRepository.GetStorefrontOrdersByUserId(userId, search);
        }

        [Timed("service.orders.get_list_by_user", "Time taken to get order list by user")]
        public virtual async Task<List<Models.DTOs.Storefront.OrderListItemDto>> GetStorefrontOrderListByUserIdAsync(string userId, string search = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderListByUserIdAsync(userId, search, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.orders.get_list_by_user_sync")]
        public virtual List<Models.DTOs.Storefront.OrderListItemDto> GetStorefrontOrderListByUserId(string userId, string search = "")
        {
            return OrderRepository.GetStorefrontOrderListByUserId(userId, search);
        }

        public async Task<Models.DTOs.Storefront.StorefrontOrderConfirmationDto> GetStorefrontOrderConfirmationByIdAsync(int orderId, string restrictToUserId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderConfirmationByIdAsync(orderId, restrictToUserId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Models.DTOs.Storefront.StorefrontOrderConfirmationDto> GetStorefrontOrderConfirmationByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderConfirmationByOrderNumberAsync(orderNumber, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.orders.get_stats_by_user", "Time taken to get order stats by user")]
        public virtual async Task<Models.DTOs.Storefront.OrderStatsDto> GetStorefrontOrderStatsByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OrderRepository.GetStorefrontOrderStatsByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Admin / Change-Tracking Methods (Full Entities)

        public void DeleteOrderById(int id)
        {
            var order = GetSingle(id);
            int shippingAddressId = order?.ShippingAddressId ?? 0;
            int billingAddressId = order?.BillingAddressId ?? 0;

            OrderProductService.DeleteOrderProductsByOrderId(id);
            DeleteById(id);

            if (AddressService != null)
            {
                if (shippingAddressId > 0 && !OrderRepository.FindBy(o => o.ShippingAddressId == shippingAddressId || o.BillingAddressId == shippingAddressId).Any())
                {
                    AddressService.DeleteById(shippingAddressId);
                }

                if (billingAddressId > 0 && billingAddressId != shippingAddressId && !OrderRepository.FindBy(o => o.ShippingAddressId == billingAddressId || o.BillingAddressId == billingAddressId).Any())
                {
                    AddressService.DeleteById(billingAddressId);
                }
            }
        }

        public async Task DeleteOrderByIdAsync(int id)
        {
            var order = await GetSingleAsync(id).ConfigureAwait(false);
            int shippingAddressId = order?.ShippingAddressId ?? 0;
            int billingAddressId = order?.BillingAddressId ?? 0;

            await OrderProductService.DeleteOrderProductsByOrderIdAsync(id).ConfigureAwait(false);
            await DeleteByIdAsync(id).ConfigureAwait(false);

            if (AddressService != null)
            {
                if (shippingAddressId > 0 && !(await OrderRepository.FindBy(o => o.ShippingAddressId == shippingAddressId || o.BillingAddressId == shippingAddressId).AnyAsync().ConfigureAwait(false)))
                {
                    await AddressService.DeleteByIdAsync(shippingAddressId).ConfigureAwait(false);
                }

                if (billingAddressId > 0 && billingAddressId != shippingAddressId && !(await OrderRepository.FindBy(o => o.ShippingAddressId == billingAddressId || o.BillingAddressId == billingAddressId).AnyAsync().ConfigureAwait(false)))
                {
                    await AddressService.DeleteByIdAsync(billingAddressId).ConfigureAwait(false);
                }
            }
        }

        public override void DeleteBaseEntity(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            try
            {
                foreach (String v in values)
                {
                    DeleteOrderById(v.ToInt());
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.LogError(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public override async Task DeleteBaseEntityAsync(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            try
            {
                foreach (String v in values)
                {
                    await DeleteOrderByIdAsync(v.ToInt()).ConfigureAwait(false);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.LogError(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public void DeleteByUserId(string userId)
        {
            var orderObjs = OrderRepository.GetOrdersUserId(userId, "");
            foreach (var order in orderObjs)
            {
                DeleteOrderById(order.Id);
            }
        }

        public async Task DeleteByUserIdAsync(string userId)
        {
            var orderObjs = await OrderRepository.GetOrdersUserIdAsync(userId, "").ConfigureAwait(false);
            foreach (var order in orderObjs)
            {
                await DeleteOrderByIdAsync(order.Id).ConfigureAwait(false);
            }
        }

        public Order GetByOrderGuid(string orderGuid)
        {
            var item = OrderRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = CustomerService.GetUserId(item.UserId);
            }
            return item;
        }

        public async Task<Order> GetByOrderGuidAsync(string orderGuid)
        {
            var item = await OrderRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = await CustomerService.GetUserIdAsync(item.UserId).ConfigureAwait(false);
            }
            return item;
        }

        public Order GetByPaymentId(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return null;
            }

            var item = OrderRepository.FindBy(r => r.PaymentId != null
                && r.PaymentId.Equals(paymentId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = CustomerService.GetUserId(item.UserId);
            }
            return item;
        }

        public async Task<Order> GetByPaymentIdAsync(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return null;
            }

            var item = await OrderRepository.FindBy(r => r.PaymentId != null
                && r.PaymentId.Equals(paymentId, StringComparison.OrdinalIgnoreCase)).FirstOrDefaultAsync().ConfigureAwait(false);
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = await CustomerService.GetUserIdAsync(item.UserId).ConfigureAwait(false);
            }
            return item;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            var item = OrderRepository.GetByOrderNumber(orderNumber);
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = CustomerService.GetUserId(item.UserId);
            }
            return item;
        }

        public async Task<Order> GetByOrderNumberAsync(string orderNumber)
        {
            var item = await OrderRepository.GetByOrderNumberAsync(orderNumber).ConfigureAwait(false);
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = await CustomerService.GetUserIdAsync(item.UserId).ConfigureAwait(false);
            }
            return item;
        }

        public Order GetOrderById(int id)
        {
            var item = OrderRepository.GetOrderById(id);
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = CustomerService.GetUserId(item.UserId);
            }
            return item;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var item = await OrderRepository.GetOrderByIdAsync(id).ConfigureAwait(false);
            if (item != null && !string.IsNullOrEmpty(item.UserId))
            {
                item.Customer = await CustomerService.GetUserIdAsync(item.UserId).ConfigureAwait(false);
            }
            return item;
        }

        public List<Order> GetOrdersByUserId(string userId)
        {
            return OrderRepository.FindAll(r => r.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase), r => r.CreatedDate, OrderByType.Descending, null, null).ToList();
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await OrderRepository.FindAll(r => r.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase), r => r.CreatedDate, OrderByType.Descending, null, null).ToListAsync().ConfigureAwait(false);
        }

        public List<Order> GetOrdersUserId(string userId, string search = "")
        {
            return OrderRepository.GetOrdersUserId(userId, search);
        }

        public async Task<List<Order>> GetOrdersUserIdAsync(string userId, string search = "")
        {
            return await OrderRepository.GetOrdersUserIdAsync(userId, search).ConfigureAwait(false);
        }

        #endregion
    }
}