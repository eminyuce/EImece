using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.Owin.Security.OAuth;
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
            item.BirthDate = model.BirthDate;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.Position = 1;
            item.Lang = 1;
            item.IsPermissionGranted = true;
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
            var resultList =  result.ToList();
            foreach (var item in resultList)
            {
                GetUserFields(item);
            }
            if (!String.IsNullOrEmpty(search))
            {
                resultList = resultList.Where(r => r.Email.Contains(search) || r.Name.Contains(search) || r.Surname.Contains(search)).ToList();
            }

            return resultList;
        }

        public void GetUserFields(Customer item)
        {
            if (item != null)
            {
                var user = UsersService.GetUser(item.UserId);
                item.Email = user.Email;
                item.Name = user.FirstName;
                item.Surname = user.LastName;
            }
        }
    }
}