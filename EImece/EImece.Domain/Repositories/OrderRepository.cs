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
                        ProductName = op.ProductName,
                        ProductCode = op.ProductCode,
                        CategoryName = op.CategoryName,
                        Quantity = op.Quantity,
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
                        ProductName = op.ProductName,
                        ProductCode = op.ProductCode,
                        CategoryName = op.CategoryName,
                        Quantity = op.Quantity,
                        TotalPrice = op.TotalPrice,
                        ProductSalePrice = op.ProductSalePrice,
                        ProductSpecItems = op.ProductSpecItems,
                        Product = op.Product != null ? new EImece.Domain.Models.DTOs.Storefront.StorefrontProductCardDto
                        {
                            Id = op.Product.Id,
                            Name = op.Product.Name,
                            ProductCategoryId = op.Product.ProductCategoryId,
                            ProductCategoryName = op.Product.ProductCategory != null ? op.Product.ProductCategory.Name : op.CategoryName,
                            MainImageId = op.Product.MainImageId,
                            Price = op.Product.Price,
                            Discount = op.Product.Discount,
                            Rating = op.Product.Rating
                        } : null
                    }).ToList()
                };
            }
        }


        private static Expression<Func<Address, AddressDto>> AddressDtoProjection
        {
            get
            {
                return a => new AddressDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    City = a.City,
                    Country = a.Country,
                    District = a.District,
                    Street = a.Street,
                    ZipCode = a.ZipCode,
                    Description = a.Description,
                    AddressType = a.AddressType
                };
            }
        }

        private async Task FillAddressesAsync(OrderDto dto, CancellationToken cancellationToken)
        {
            if (dto == null)
            {
                return;
            }
            if (dto.ShippingAddressId > 0)
            {
                dto.ShippingAddress = await EImeceDbContext.Addresses.AsNoTracking()
                    .Where(a => a.Id == dto.ShippingAddressId)
                    .Select(AddressDtoProjection)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            if (dto.BillingAddressId > 0)
            {
                dto.BillingAddress = await EImeceDbContext.Addresses.AsNoTracking()
                    .Where(a => a.Id == dto.BillingAddressId)
                    .Select(AddressDtoProjection)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private void FillAddresses(OrderDto dto)
        {
            if (dto == null)
            {
                return;
            }
            if (dto.ShippingAddressId > 0)
            {
                dto.ShippingAddress = EImeceDbContext.Addresses.AsNoTracking()
                    .Where(a => a.Id == dto.ShippingAddressId)
                    .Select(AddressDtoProjection)
                    .FirstOrDefault();
            }
            if (dto.BillingAddressId > 0)
            {
                dto.BillingAddress = EImeceDbContext.Addresses.AsNoTracking()
                    .Where(a => a.Id == dto.BillingAddressId)
                    .Select(AddressDtoProjection)
                    .FirstOrDefault();
            }
        }

        public async Task<OrderDto> GetStorefrontOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dto = await EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.Id == id)
                .Select(OrderDetailProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            await FillAddressesAsync(dto, cancellationToken).ConfigureAwait(false);
            return dto;
        }

        public OrderDto GetStorefrontOrderById(int id)
        {
            var dto = EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.Id == id)
                .Select(OrderDetailProjection)
                .FirstOrDefault();
            FillAddresses(dto);
            return dto;
        }

        public async Task<OrderDto> GetStorefrontOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dto = await EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderNumber == orderNumber)
                .Select(OrderDetailProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            await FillAddressesAsync(dto, cancellationToken).ConfigureAwait(false);
            return dto;
        }

        public OrderDto GetStorefrontOrderByOrderNumber(string orderNumber)
        {
            var dto = EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderNumber == orderNumber)
                .Select(OrderDetailProjection)
                .FirstOrDefault();
            FillAddresses(dto);
            return dto;
        }

        public async Task<OrderDto> GetStorefrontOrderByGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dto = await EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderGuid == orderGuid)
                .Select(OrderDetailProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            await FillAddressesAsync(dto, cancellationToken).ConfigureAwait(false);
            return dto;
        }

        public OrderDto GetStorefrontOrderByGuid(string orderGuid)
        {
            var dto = EImeceDbContext.Orders.AsNoTracking()
                .Where(o => o.OrderGuid == orderGuid)
                .Select(OrderDetailProjection)
                .FirstOrDefault();
            FillAddresses(dto);
            return dto;
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

        public async Task<List<Models.DTOs.Storefront.OrderListItemDto>> GetStorefrontOrderListByUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken))
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
                .Select(o => new Models.DTOs.Storefront.OrderListItemDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderStatus = o.OrderStatus,
                    CreatedDate = o.CreatedDate,
                    PaidPrice = o.PaidPrice,
                    ShipmentTrackingNumber = o.ShipmentTrackingNumber,
                    ShipmentCompanyName = o.ShipmentCompanyName
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<Models.DTOs.Storefront.OrderListItemDto> GetStorefrontOrderListByUserId(string userId, string search)
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
                .Select(o => new Models.DTOs.Storefront.OrderListItemDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderStatus = o.OrderStatus,
                    CreatedDate = o.CreatedDate,
                    PaidPrice = o.PaidPrice,
                    ShipmentTrackingNumber = o.ShipmentTrackingNumber,
                    ShipmentCompanyName = o.ShipmentCompanyName
                }).ToList();
        }

        #endregion

        public Order GetOrderById(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public async Task<Order> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = await GetSingleIncludingAsync(id, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);

            return item;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            var includeProperties = GetIncludePropertyExpressionList();
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