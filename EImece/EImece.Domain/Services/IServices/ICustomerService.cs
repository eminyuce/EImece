using EImece.Domain.Entities;
using EImece.Models;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ICustomerService : IBaseEntityService<Customer>
    {
        void SaveRegisterViewModel(string userId, RegisterViewModel model);

        Customer GetUserId(string userId);

        CustomerDto GetUserIdDto(string userId);

        CustomerDto SaveOrEditCustomerDto(CustomerDto customerDto);

        void DeleteByUserId(string userId);

        void SaveCustomerTypeToNormal(string userId);

        List<Customer> GetCustomerServices(string search);

        void GetUserFields(Customer item);
    }
}