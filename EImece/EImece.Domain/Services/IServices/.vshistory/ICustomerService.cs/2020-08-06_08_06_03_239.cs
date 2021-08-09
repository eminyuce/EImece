using EImece.Domain.Entities;
using EImece.Models;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ICustomerService : IBaseEntityService<Customer>
    {
        void SaveRegisterViewModel(string userId, RegisterViewModel model);

        Customer GetUserId(string userId);

        void DeleteByUserId(string userId);

        void SaveShippingAddress(string userId);

        List<Customer> GetCustomerServices(string search);

        void GetUserFields(Customer item);
    }
}