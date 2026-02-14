using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Services
{
    public class CustomerService : BaseEntityService<Customer>, ICustomerService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IOrderService OrderService { get; set; }

        private ICustomerRepository CustomerRepository { get; set; }

        private IAddressService AddressService { get; set; }

        [Inject]
        public UsersService UsersService { get; set; }

        public CustomerService(ICustomerRepository repository, IAddressService addressService) : base(repository)
        {
            Logger.Info("CustomerService initialized.");
            CustomerRepository = repository;
            AddressService = addressService;
        }

        public void SaveRegisterViewModel(string userId, RegisterViewModel model)
        {
            Logger.Info($"Saving RegisterViewModel for user: {userId}");
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
                Logger.Info("Customer successfully saved.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error saving RegisterViewModel.");
                throw;
            }
        }

        public Customer GetUserId(string userId)
        {
            Logger.Info($"Retrieving customer by userId: {userId}");
            var item = CustomerRepository.GetUserId(userId);
            GetUserFields(item);
            return item;
        }


        public CustomerDto GetUserIdDto(string userId)
        {
            var customer = GetUserId(userId);
            return DtoMappingService.MapTo<CustomerDto>(customer);
        }

        public CustomerDto SaveOrEditCustomerDto(CustomerDto customerDto)
        {
            var customer = DtoMappingService.MapTo<Customer>(customerDto);
            var savedCustomer = SaveOrEditEntity(customer);
            return DtoMappingService.MapTo<CustomerDto>(savedCustomer);
        }
        public void DeleteByUserId(string userId)
        {
            Logger.Info($"Deleting customer by userId: {userId}");
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                DeleteEntity(customer);
                Logger.Info("Customer successfully deleted.");
            }
            else
            {
                Logger.Warn("Customer not found.");
            }
        }

        public void SaveCustomerTypeToNormal(string userId)
        {
            Logger.Info($"Updating customer type to Normal for userId: {userId}");
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                customer.CustomerType = (int)EImeceCustomerType.Normal;
                customer.GsmNumber = GeneralHelper.CheckGsmNumber(customer.GsmNumber);
                SaveOrEditEntity(customer);
                Logger.Info("Customer type updated successfully.");
            }
            else
            {
                Logger.Warn("Customer not found.");
            }
        }

        public List<Customer> GetCustomerServices(string search)
        {
            Logger.Info($"Retrieving customer services with search term: {search}");
            search = search.ToStr().Trim();
            var result = CustomerRepository.GetAll().Where(r => r.CustomerType == (int)EImeceCustomerType.Normal || r.CustomerType == (int)EImeceCustomerType.ShoppingWithoutAccount).ToList();
            var allOrders = OrderService.GetAll().Where(r => r.OrderType == (int)EImeceOrderType.NormalOrder || r.OrderType == (int)EImeceOrderType.BuyWithNoAccountCreation).ToList();
            var resultList = result.ToList();

            if (resultList.IsNotEmpty())
            {
                foreach (var item in resultList)
                {
                    item.Orders = allOrders.Where(r => r.UserId.Equals(item.UserId, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    item.OrderLatestDate = item.Orders.IsNotEmpty() ? item.Orders.Max(T => T.CreatedDate) : DateTime.Now.AddYears(-2);
                    GetUserFields(item);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    resultList = resultList.Where(r =>
                    r.Email.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                       string.Format("{0} {1}", r.Name, r.Surname).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                                           .ToList();
                }
            }

            resultList = resultList.OrderByDescending(r => r.OrderLatestDate).ThenByDescending(r => r.CreatedDate).ToList();
            Logger.Info("Customer services retrieved successfully.");
            return resultList;
        }

        public void GetUserFields(Customer item)
        {
            if (item == null)
            {
                Logger.Warn("GetUserFields called with a null item.");
                return;
            }
            Logger.Info($"Fetching user fields for userId: {item.UserId}");
            var user = UsersService.GetUser(item.UserId);
            if (user != null)
            {
                item.Email = user.Email;
                item.Name = user.FirstName;
                item.Surname = user.LastName;
                Logger.Info("User fields populated successfully.");
            }
            else
            {
                Logger.Warn("User not found in UsersService.");
            }
        }
    }
}