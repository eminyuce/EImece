using EImece.Domain.Entities;
using EImece.Models;

namespace EImece.Domain.Services.IServices
{
    public interface ICustomerService : IBaseEntityService<Customer>
    {
        void SaveRegisterViewModel(string userId,RegisterViewModel model);
        Customer GetUserId(string userId);
        void DeleteByUserId(string userId);
        void SaveShippingAddress(string userId, int addressId);
    }
}