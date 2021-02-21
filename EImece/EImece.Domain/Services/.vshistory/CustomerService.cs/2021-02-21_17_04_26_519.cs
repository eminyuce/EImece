using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
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
            CustomerRepository = repository;
            AddressService = addressService;
        }

        public void SaveRegisterViewModel(string userId, RegisterViewModel model)
        {
            var item = new Customer();

            item.UserId = userId;
            item.Name = model.FirstName;
            item.GsmNumber = model.PhoneNumber;
            item.IdentityNumber = "";
            item.Ip = GeneralHelper.GetIpAddress();
            item.IsActive = true;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.Position = 1;
            item.Lang = 1;
            item.IsPermissionGranted = model.IsPermissionGranted;
            item.Street = "";
            item.District = "";
            item.City = "";
            item.Country = "";
            item.ZipCode = "";
            CustomerRepository.SaveOrEdit(item);
        }

        public Customer GetUserId(string userId)
        {
            var item = CustomerRepository.GetUserId(userId);
            GetUserFields(item);
            return item;
        }

        public void DeleteByUserId(string userId)
        {
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                DeleteEntity(customer);
            }
        }

        public void SaveShippingAddress(string userId)
        {
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                SaveOrEditEntity(customer);
            }
        }

        public List<Customer> GetCustomerServices(string search)
        {
            var result = CustomerRepository.GetAll();
            var allOrders = OrderService.GetAll();
            var resultList = result.ToList();
            if (resultList.IsNotEmpty())
            {
                foreach (var item in resultList)
                {
                    item.Orders = allOrders.Where(r => r.UserId.Equals(item.UserId, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    GetUserFields(item);
                }
            }
            if (!String.IsNullOrEmpty(search))
            {
                resultList = resultList.Where(r => r.Email.Contains(search) || string.Format("{0} {1}", r.Name, r.Surname).Contains(search)).ToList();
            }
            return resultList.OrderByDescending(r => r.CreatedDate).ToList();
        }

        public void GetUserFields(Customer item)
        {
            if (item != null)
            {
                var user = UsersService.GetUser(item.UserId);
                if (user == null)
                    return;
                item.Email = user.Email;
                item.Name = user.FirstName;
                item.Surname = user.LastName;
            }
        }
    }
}