using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class OrderService : BaseEntityService<Order>, IOrderService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOrderRepository OrderRepository;

        private readonly IOrderProductService OrderProductService;

        private ICustomerService CustomerService;

        public OrderService(IOrderRepository repository, ICustomerService customerService, IOrderProductService orderProductService) : base(repository)
        {
            OrderRepository = repository;
            OrderProductService = orderProductService;
            this.CustomerService = customerService;
        }

        public void DeleteOrderById(int id)
        {
            OrderProductService.DeleteOrderProductsByOrderId(id);
            DeleteById(id);
        }

        public async Task DeleteOrderByIdAsync(int id)
        {
            await OrderProductService.DeleteOrderProductsByOrderIdAsync(id).ConfigureAwait(false);
            await DeleteByIdAsync(id).ConfigureAwait(false);
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
                Logger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
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
                Logger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
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
    }
}