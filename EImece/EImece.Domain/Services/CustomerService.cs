using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class CustomerService : BaseEntityService<Customer>, ICustomerService
    {
        private readonly IOrderService OrderService;
        private readonly IOrderRepository OrderRepository;
        private readonly ICustomerRepository CustomerRepository;
        private readonly IAddressService AddressService;
        private readonly IUserRepository UserRepository;
        private readonly ApplicationUserManager UserManager;

        public CustomerService(ICustomerRepository repository,
            ILogger<CustomerService> logger,
            IAddressService addressService = null,
            IOrderRepository orderRepository = null,
            IOrderService orderService = null,
            IUserRepository userRepository = null,
            ApplicationUserManager userManager = null) : base(repository, logger)
        {
            CustomerRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            AddressService = addressService;
            OrderRepository = orderRepository;
            OrderService = orderService;
            UserRepository = userRepository;
            UserManager = userManager;
            Logger.LogDebug("CustomerService initialized.");
        }

        public void SaveRegisterViewModel(string userId, CustomerRegistrationDto model)
        {
            Logger.LogDebug($"Saving RegisterViewModel for user: {userId}");
            try
            {
                var item = new Customer
                {
                    UserId = userId,
                    Name = model.FirstName,
                    GsmNumber = GeneralHelper.CheckGsmNumber(model.PhoneNumber),
                    IdentityNumber = "",
                    Ip = GeneralHelper.GetIpAddress(),
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Position = 1,
                    Lang = 1,
                    IsPermissionGranted = model.IsPermissionGranted,
                    Street = "",
                    District = "",
                    City = "",
                    Country = "",
                    ZipCode = ""
                };

                CustomerRepository.SaveOrEdit(item);
                Logger.LogDebug("Customer successfully saved. UserId={0}", userId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving RegisterViewModel.");
                throw;
            }
        }

        public async Task SaveRegisterViewModelAsync(string userId, CustomerRegistrationDto model)
        {
            Logger.LogDebug($"Saving RegisterViewModel for user: {userId}");
            try
            {
                var item = new Customer
                {
                    UserId = userId,
                    Name = model.FirstName,
                    GsmNumber = GeneralHelper.CheckGsmNumber(model.PhoneNumber),
                    IdentityNumber = "",
                    Ip = GeneralHelper.GetIpAddress(),
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Position = 1,
                    Lang = 1,
                    IsPermissionGranted = model.IsPermissionGranted,
                    Street = "",
                    District = "",
                    City = "",
                    Country = "",
                    ZipCode = ""
                };

                await CustomerRepository.SaveOrEditAsync(item).ConfigureAwait(false);
                Logger.LogDebug("Customer successfully saved. UserId={0}", userId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving RegisterViewModel.");
                throw new InvalidOperationException("Error saving RegisterViewModel.", ex);
            }
        }

        [Timed("service.customers.get_by_user_sync")]
        public virtual Customer GetUserId(string userId)
        {
            Logger.LogDebug($"Retrieving customer by userId: {userId}");
            var item = CustomerRepository.GetUserId(userId);
            GetUserFields(item);
            return item;
        }

        [Timed("service.customers.get_by_user")]
        public virtual async Task<Customer> GetUserIdAsync(string userId)
        {
            Logger.LogDebug($"Retrieving customer by userId: {userId}");
            var item = await CustomerRepository.GetUserIdAsync(userId).ConfigureAwait(false);
            await GetUserFieldsAsync(item).ConfigureAwait(false);
            return item;
        }

        [Timed("service.customers.get_summary_by_user", "Time taken to get customer summary by user")]
        public virtual async Task<Models.DTOs.Storefront.CustomerSummaryDto> GetStorefrontCustomerSummaryByUserIdAsync(string userId)
        {
            return await CustomerRepository.GetStorefrontCustomerSummaryByUserIdAsync(userId).ConfigureAwait(false);
        }

        [Timed("service.customers.get_profile_by_user")]
        public virtual async Task<Models.DTOs.CustomerDto> GetStorefrontCustomerProfileByUserIdAsync(string userId)
        {
            return await CustomerRepository.GetStorefrontCustomerProfileByUserIdAsync(userId).ConfigureAwait(false);
        }

        public void DeleteByUserId(string userId)
        {
            Logger.LogDebug($"Deleting customer by userId: {userId}");
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                DeleteEntity(customer);
                Logger.LogInformation("Customer successfully deleted.");
            }
            else
            {
                Logger.LogWarning(Constants.CustomerNotFoundMessage);
            }
        }

        public async Task DeleteByUserIdAsync(string userId)
        {
            Logger.LogDebug($"Deleting customer by userId: {userId}");
            var customer = await CustomerRepository.GetUserIdAsync(userId).ConfigureAwait(false);
            if (customer != null)
            {
                await DeleteEntityAsync(customer).ConfigureAwait(false);
                Logger.LogInformation("Customer successfully deleted.");
            }
            else
            {
                Logger.LogWarning(Constants.CustomerNotFoundMessage);
            }
        }

        public virtual void SaveCustomerTypeToNormal(string userId)
        {
            Logger.LogDebug($"Updating customer type to Normal for userId: {userId}");
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                customer.CustomerType = (int)EImeceCustomerType.Normal;
                customer.GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber);
                SaveOrEditEntity(customer);
                Logger.LogDebug("Customer type updated successfully.");
            }
            else
            {
                Logger.LogWarning(Constants.CustomerNotFoundMessage);
            }
        }

        public virtual async Task SaveCustomerTypeToNormalAsync(string userId)
        {
            Logger.LogDebug($"Updating customer type to Normal for userId: {userId}");
            // Targeted 2-column update — no full-entity load/save round trip
            var updated = await CustomerRepository.PromoteCustomerToNormalTypeAsync(userId, (int)EImeceCustomerType.Normal).ConfigureAwait(false);
            if (updated)
            {
                Logger.LogDebug("Customer type updated successfully.");
            }
            else
            {
                Logger.LogWarning(Constants.CustomerNotFoundMessage);
            }
        }

        public List<Customer> GetCustomerServices(string search)
        {
            Logger.LogDebug($"Retrieving customer services with search term: {search}");
            search = search.ToStr().Trim();

            // AsNoTracking avoids change-tracker overhead for read-only admin grid.
            var customers = CustomerRepository.GetAll().AsNoTracking()
                .Where(r => r.CustomerType == (int)EImeceCustomerType.Normal || r.CustomerType == (int)EImeceCustomerType.ShoppingWithoutAccount)
                .ToList();

            if (!customers.IsNotEmpty())
            {
                return new List<Customer>();
            }

            var userIds = customers.Where(c => !string.IsNullOrWhiteSpace(c.UserId)).Select(c => c.UserId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // Single round-trip for lightweight order aggregates (UserId, CreatedDate only) instead of full Order entities.
            var orderQuery = (OrderRepository != null ? OrderRepository.GetAll().AsNoTracking() : OrderService.GetAll().AsQueryable())
                .Where(r => r.OrderType == (int)EImeceOrderType.NormalOrder || r.OrderType == (int)EImeceOrderType.BuyWithNoAccountCreation)
                .Select(r => new { r.UserId, r.CreatedDate, r.Id });
            var orderRows = orderQuery.ToList();

            var ordersByUser = orderRows
                .GroupBy(r => r.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            // Batch UsersService lookup: 1 query instead of N per customer.
            var usersDict = BuildUsersDictionary(userIds);

            var resultList = new List<Customer>(customers.Count);
            foreach (var item in customers)
            {
                if (ordersByUser.TryGetValue(item.UserId ?? string.Empty, out var uOrders))
                {
                    item.Orders = uOrders.Select(o => new Order { Id = o.Id, UserId = o.UserId, CreatedDate = o.CreatedDate }).ToList();
                    item.OrderLatestDate = uOrders.Max(t => t.CreatedDate);
                }
                else
                {
                    item.Orders = new List<Order>();
                    item.OrderLatestDate = DateTime.Now.AddYears(-2);
                }

                if (usersDict.TryGetValue(item.UserId ?? string.Empty, out var u))
                {
                    item.Email = u.Email;
                    item.Name = u.FirstName;
                    item.Surname = u.LastName;
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                resultList = customers.Where(r =>
                    (r.Email != null && r.Email.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    string.Format("{0} {1}", r.Name, r.Surname).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();
            }
            else
            {
                resultList = customers.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();
            }

            Logger.LogDebug("Customer services retrieved successfully. Customers={0} OrdersRows={1}", customers.Count, orderRows.Count);
            return resultList;
        }

        public async Task<List<Customer>> GetCustomerServicesAsync(string search)
        {
            Logger.LogDebug($"Retrieving customer services with search term: {search}");
            search = search.ToStr().Trim();

            var customers = await CustomerRepository.GetAll().AsNoTracking()
                .Where(r => r.CustomerType == (int)EImeceCustomerType.Normal || r.CustomerType == (int)EImeceCustomerType.ShoppingWithoutAccount)
                .ToListAsync().ConfigureAwait(false);

            if (!customers.IsNotEmpty())
            {
                return new List<Customer>();
            }

            var userIds = customers.Where(c => !string.IsNullOrWhiteSpace(c.UserId)).Select(c => c.UserId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var orderQueryAsync = (OrderRepository != null ? OrderRepository.GetAll().AsNoTracking() : OrderService.GetAll().AsQueryable())
                .Where(r => r.OrderType == (int)EImeceOrderType.NormalOrder || r.OrderType == (int)EImeceOrderType.BuyWithNoAccountCreation)
                .Select(r => new { r.UserId, r.CreatedDate, r.Id });
            var orderRows = await orderQueryAsync.ToListAsync().ConfigureAwait(false);

            var ordersByUser = orderRows
                .GroupBy(r => r.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var usersDict = await BuildUsersDictionaryAsync(userIds).ConfigureAwait(false);

            foreach (var item in customers)
            {
                if (ordersByUser.TryGetValue(item.UserId ?? string.Empty, out var uOrders))
                {
                    item.Orders = uOrders.Select(o => new Order { Id = o.Id, UserId = o.UserId, CreatedDate = o.CreatedDate }).ToList();
                    item.OrderLatestDate = uOrders.Max(t => t.CreatedDate);
                }
                else
                {
                    item.Orders = new List<Order>();
                    item.OrderLatestDate = DateTime.Now.AddYears(-2);
                }

                if (usersDict.TryGetValue(item.UserId ?? string.Empty, out var u))
                {
                    item.Email = u.Email;
                    item.Name = u.FirstName;
                    item.Surname = u.LastName;
                }
            }

            List<Customer> resultList;
            if (!string.IsNullOrEmpty(search))
            {
                resultList = customers.Where(r =>
                    (r.Email != null && r.Email.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    string.Format("{0} {1}", r.Name, r.Surname).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();
            }
            else
            {
                resultList = customers.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();
            }

            Logger.LogDebug("Customer services retrieved successfully. Customers={0} OrdersRows={1}", customers.Count, orderRows.Count);
            return resultList;
        }

        private Dictionary<string, (string Email, string FirstName, string LastName)> BuildUsersDictionary(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0 || UserManager == null)
            {
                return new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase);
            }
            try
            {
                return UserManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                    .ToList()
                    .ToDictionary(x => x.Id, x => (x.Email, x.FirstName, x.LastName), StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "BuildUsersDictionary batch fetch failed, falling back to per-item lookup.");
                return new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task<Dictionary<string, (string Email, string FirstName, string LastName)>> BuildUsersDictionaryAsync(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0 || UserManager == null)
            {
                return new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase);
            }
            try
            {
                var rows = await UserManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                    .ToListAsync().ConfigureAwait(false);
                return rows.ToDictionary(x => x.Id, x => (x.Email, x.FirstName, x.LastName), StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "BuildUsersDictionaryAsync batch fetch failed, falling back to per-item lookup.");
                return new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public void GetUserFields(Customer item)
        {
            if (item == null)
            {
                Logger.LogWarning("GetUserFields called with a null item.");
                return;
            }
            Logger.LogDebug($"Fetching user fields for userId: {item.UserId}");
            var user = UserRepository.GetById(item.UserId);
            if (user != null)
            {
                item.Email = user.Email;
                item.Name = user.FirstName;
                item.Surname = user.LastName;
                Logger.LogDebug("User fields populated successfully.");
            }
            else
            {
                Logger.LogWarning("User not found in UserRepository.");
            }
        }

        public async Task GetUserFieldsAsync(Customer item)
        {
            if (item == null)
            {
                Logger.LogWarning("GetUserFields called with a null item.");
                return;
            }
            Logger.LogDebug($"Fetching user fields for userId: {item.UserId}");
            var user = await UserRepository.GetByIdAsync(item.UserId).ConfigureAwait(false);
            if (user != null)
            {
                item.Email = user.Email;
                item.Name = user.FirstName;
                item.Surname = user.LastName;
                Logger.LogDebug("User fields populated successfully.");
            }
            else
            {
                Logger.LogWarning("User not found in UserRepository.");
            }
        }

        public override async Task DeleteBaseEntityAsync(List<string> values)
        {
            await DeleteCustomersAsync(values).ConfigureAwait(false);
        }

        public async Task<List<string>> DeleteCustomersAsync(List<string> userIds, string currentUserId = null)
        {
            var deleted = new List<string>();
            if (userIds == null || userIds.Count == 0)
            {
                return deleted;
            }

            Logger.LogDebug($"DeleteCustomersAsync called for {userIds.Count} userIds");
            foreach (var userId in userIds.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(currentUserId)
                    && string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (UserManager != null)
                {
                    var user = await UserRepository.GetByIdAsync(userId).ConfigureAwait(false);
                    if (user != null)
                    {
                        var roles = await UserManager.GetRolesAsync(userId).ConfigureAwait(false);
                        var isCustomer = roles != null
                            && roles.Any(r => r.Equals(Constants.CustomerRole, StringComparison.OrdinalIgnoreCase));
                        if (!isCustomer && roles != null && roles.Count > 0)
                        {
                            // Safety: customer grid must not bulk-delete staff accounts.
                            continue;
                        }

                        await UserManager.DeleteAsync(user).ConfigureAwait(false);
                    }
                }

                await DeleteByUserIdAsync(userId).ConfigureAwait(false);
                if (OrderService != null)
                {
                    await OrderService.DeleteByUserIdAsync(userId).ConfigureAwait(false);
                }
                deleted.Add(userId);
            }

            Logger.LogInformation($"DeleteCustomersAsync finished. Total deleted: {deleted.Count}");
            return deleted;
        }
    }
}