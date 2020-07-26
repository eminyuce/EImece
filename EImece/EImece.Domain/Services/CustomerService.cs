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

        public CustomerService(ICustomerRepository repository) : base(repository)
        {
            CustomerRepository = repository;
        }

        public void SaveRegisterViewModel(RegisterViewModel model)
        {
            var item = new Customer();

            item.UserId = model.GetUser().Id;
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
    }
}