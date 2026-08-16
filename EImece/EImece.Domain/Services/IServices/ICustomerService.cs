using EImece.Domain.Entities;
using EImece.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ICustomerService : IBaseEntityService<Customer>
    {
        void SaveRegisterViewModel(string userId, RegisterViewModel model);

        Task SaveRegisterViewModelAsync(string userId, RegisterViewModel model);

        Customer GetUserId(string userId);

        Task<Customer> GetUserIdAsync(string userId);

        void DeleteByUserId(string userId);

        Task DeleteByUserIdAsync(string userId);

        void SaveCustomerTypeToNormal(string userId);

        Task SaveCustomerTypeToNormalAsync(string userId);

        List<Customer> GetCustomerServices(string search);

        Task<List<Customer>> GetCustomerServicesAsync(string search);

        void GetUserFields(Customer item);

        Task GetUserFieldsAsync(Customer item);

        Task<List<string>> DeleteCustomersAsync(List<string> userIds, string currentUserId = null);
    }
}