using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class OrderRepository : BaseEntityRepository<Order>, IOrderRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ICustomerRepository CustomerRepository { get; set; }

        public OrderRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        private static Expression<Func<Order, OrderDto>> OrderSummaryProjection
        {
            get
            {
                return o => new OrderDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    OrderNumber = o.OrderNumber,
                    OrderGuid = o.OrderGuid,
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate,
                    DeliveryDate = o.DeliveryDate,
                    OrderStatus = o.OrderStatus,
                    OrderType = o.OrderType,
                    UserId = o.UserId,
                    Price = o.Price,
                    PaidPrice = o.PaidPrice,
                    PaidPriceDecimal = o.PaidPriceDecimal,
                    CargoPrice = o.CargoPrice,
                    Currency = o.Currency,
                    PaymentStatus = o.PaymentStatus,
                    ShipmentCompanyName = o.ShipmentCompanyName,
                    ShipmentTrackingNumber = o.ShipmentTrackingNumber,
                    ShippingAddressId = o.ShippingAddressId,
                    BillingAddressId = o.BillingAddressId,
                    IsActive = o.IsActive,
                    Position = o.Position,
                    Lang = o.Lang,
                    OrderProducts = o.OrderProducts.Select(op => new OrderProductDto
                    {
                        Id = op.Id,
                        OrderId = op.OrderId,
                        ProductId = op.ProductId,
                        ProductName = op.Product != null ? op.Product.Name : string.Empty,
                        ProductCode = op.Product != null ? op.Product.ProductCode : string.Empty,
                        CategoryName = op.Product != null && op.Product.ProductCategory != null ? op.Product.ProductCategory.Name : string.Empty,
                        Quantity = op.Quantity,
                        Price = op.Price,
                        TotalPrice = op.TotalPrice,
                        ProductSalePrice = op.ProductSalePrice,
                        ProductSpecItems = op.ProductSpecItems
                    }).ToList()
                };
            }
        }

        private static Expression<Func<Order, OrderDto>> OrderDetailProjection
        {
            get
            {
                return o => new OrderDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    OrderNumber = o.OrderNumber,
                    OrderGuid = o.OrderGuid,
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate,
                    DeliveryDate = o.DeliveryDate,
                    OrderStatus = o.OrderStatus,
                    OrderType = o.OrderType,
                    UserId = o.UserId,
                    Price = o.Price,
                    PaidPrice = o.PaidPrice,
                    PaidPriceDecimal = o.PaidPriceDecimal,
                    CargoPrice = o.CargoPrice,
                    Currency = o.Currency,
                    PaymentStatus = o.PaymentStatus,
                    PaymentId = o.PaymentId,
                    Coupon = o.Coupon,
                    CouponDiscount = o.CouponDiscount,
                    OrderComments = o.OrderComments,
                    AdminOrderNote = o.AdminOrderNote,
                    CardFamily = o.CardFamily,
                    CardAssociation = o.CardAssociation,
                    CardType = o.CardType,
                    LastFourDigits = o.LastFourDigits,
                    Installment = o.Installment,
                    InstallmentDescription = o.InstallmentDescription,
                    ShipmentCompanyName = o.ShipmentCompanyName,
                    ShipmentTrackingNumber = o.ShipmentTrackingNumber,
                    ShippingAddressId = o.ShippingAddressId,
                    BillingAddressId = o.BillingAddressId,
                    IsActive = o.IsActive,
                    Position = o.Position,
                    Lang = o.Lang,
                    ShippingAddress = o.ShippingAddress != null ? new AddressDto
                    {
                        Id = o.ShippingAddress.Id,
                        Name = o.ShippingAddress.Name,
                        City = o.ShippingAddress.City,
                        Country = o.ShippingAddress.Country,
                        District = o.ShippingAddress.District,
                        Street = o.ShippingAddress.Street,
                        ZipCode = o.ShippingAddress.ZipCode,
                        Description = o.ShippingAddress.Description,
                        AddressInfo = o.ShippingAddress.AddressInfo,
                        AddressType = o.ShippingAddress.AddressType
                    } : null,
                    BillingAddress = o.BillingAddress != null ? new AddressDto
                    {
                        Id = o.BillingAddress.Id,
                        Name = o.BillingAddress.Name,
                        City = o.BillingAddress.City,
                        Country = o.BillingAddress.Country,
                        District = o.BillingAddress.District,
                        Street = o.BillingAddress.Street,
                        ZipCode = o.BillingAddress.ZipCode,
                        Description = o.BillingAddress.Description,
                        AddressInfo = o.BillingAddress.AddressInfo,
                        AddressType = o.BillingAddress.AddressType
                    } : null,
                    OrderProducts = o.OrderProducts.Select(op => new OrderProductDto
                    {
                        Id = op.Id,
                        OrderId = op.OrderId,
                        ProductId = op.ProductId,
                        ProductName = op.Product != null ? op.Product.Name : string.Empty,
                        ProductCode = op.Product != null ? op.Product.ProductCode : string.Empty,
                        CategoryName = op.Product != null && op.Product.ProductCategory != null ? op.Product.ProductCategory.Name : string.Empty,
                        Quantity = op.Quantity,
                        Price = op.Price,
                        TotalPrice = op.TotalPrice,
                        ProductSalePrice = op.ProductSalePrice,
                        ProductSpecItems = op.ProductSpecItems
                    }).ToList()
                };
            }
        }

        public async Task<OrderDto> GetStorefrontOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.Id == id)
                .Select(OrderDetailProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public OrderDto GetStorefrontOrderById(int id)
        {
            return EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.Id == id)
                .Select(OrderDetailProjection)
                .FirstOrDefault();
        }

        public async Task<OrderDto> GetStorefrontOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderNumber == orderNumber)
                .Select(OrderDetailProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public OrderDto GetStorefrontOrderByOrderNumber(string orderNumber)
        {
            return EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderNumber == orderNumber)
                .Select(OrderDetailProjection)
                .FirstOrDefault();
        }

        public async Task<OrderDto> GetStorefrontOrderByGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderGuid == orderGuid)
                .Select(OrderDetailProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public OrderDto GetStorefrontOrderByGuid(string orderGuid)
        {
            return EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderGuid == orderGuid)
                .Select(OrderDetailProjection)
                .FirstOrDefault();
        }

        public async Task<List<OrderDto>> GetStorefrontOrdersByUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken))
        {
            search = search.ToStr().Trim();
            var query = EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.OrderGuid == search || o.OrderNumber == search);
            }

            return await query
                .OrderByDescending(o => o.UpdatedDate)
                .Select(OrderSummaryProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<OrderDto> GetStorefrontOrdersByUserId(string userId, string search)
        {
            search = search.ToStr().Trim();
            var query = EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.OrderGuid == search || o.OrderNumber == search);
            }

            return query
                .OrderByDescending(o => o.UpdatedDate)
                .Select(OrderSummaryProjection)
                .ToList();
        }

        #endregion

        public Order GetOrderById(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public async Task<Order> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = await GetSingleIncludingAsync(id, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);

            return item;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            Expression<Func<Order, bool>> match = r2 => r2.OrderNumber == orderNumber;
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            return orders.FirstOrDefault();
        }

        public async Task<Order> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            Expression<Func<Order, bool>> match = r2 => r2.OrderNumber == orderNumber;
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            return await orders.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<Order> GetOrdersUserId(string userId, string search)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.MainImage));
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);

            search = search.ToStr().Trim();
            Expression<Func<Order, bool>> match;
            if (string.IsNullOrEmpty(search))
            {
                match = r2 => r2.UserId == userId;
            }
            else
            {
                var term = search;
                match = r2 => r2.UserId == userId
                    && (r2.OrderGuid == term || r2.OrderNumber == term);
            }

            Expression<Func<Order, DateTime>> keySelector = t => t.UpdatedDate;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            return orders.ToList();
        }

        public async Task<List<Order>> GetOrdersUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.MainImage));
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);

            search = search.ToStr().Trim();
            Expression<Func<Order, bool>> match;
            if (string.IsNullOrEmpty(search))
            {
                match = r2 => r2.UserId == userId;
            }
            else
            {
                var term = search;
                match = r2 => r2.UserId == userId
                    && (r2.OrderGuid == term || r2.OrderNumber == term);
            }

            Expression<Func<Order, DateTime>> keySelector = t => t.UpdatedDate;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            return await orders.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}