using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using NLog;
using NPOI.SS.Formula.Functions;
using System;

namespace EImece.Domain.Services
{
    public class CustomerService : BaseEntityService<Customer>, ICustomerService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ICustomerRepository CustomerRepository { get; set; }

        private IAddressService AddressService { get; set; }

        public CustomerService(ICustomerRepository repository, IAddressService addressService) : base(repository)
        {
            CustomerRepository = repository;
            AddressService = addressService;
        }

        public void SaveRegisterViewModel(string userId,RegisterViewModel model)
        {
            var item = new Customer();

            item.UserId = userId;
            item.Name = model.FirstName;
            item.Surname = model.LastName;
            item.GsmNumber = model.PhoneNumber;
            item.Email = model.Email;
            item.IdentityNumber = "";
            item.Ip = GeneralHelper.GetIpAddress();
            item.IsActive = true;
            item.CreatedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            item.Position = 1;
            item.Lang = 1;
            item.IsPermissionGranted = true;
            CustomerRepository.SaveOrEdit(item);
        }

        public Customer GetUserId(string userId)
        {
            return CustomerRepository.GetUserId(userId);
        }

        public void DeleteByUserId(string userId)
        {
            var customer = CustomerRepository.GetUserId(userId);
            if(customer != null)
            {
                AddressService.DeleteById(customer.AddressId);
                DeleteEntity(customer);
            }

        }

        public void SaveShippingAddress(string userId, int addressId)
        {
            var customer = CustomerRepository.GetUserId(userId);
            if (customer != null)
            {
                customer.AddressId = addressId;
                SaveOrEditEntity(customer);
            }
        }
    }
}