using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ICustomerService : IBaseEntityService<Customer>
    {
        void SaveRegisterViewModel(string userId, CustomerRegistrationDto model);

        Task SaveRegisterViewModelAsync(string userId, CustomerRegistrationDto model);

        Customer GetUserId(string userId);

        Task<Customer> GetUserIdAsync(string userId);

        Task<EImece.Domain.Models.DTOs.Storefront.CustomerSummaryDto> GetStorefrontCustomerSummaryByUserIdAsync(string userId);

        Task<EImece.Domain.Models.DTOs.CustomerDto> GetStorefrontCustomerProfileByUserIdAsync(string userId);

        void DeleteByUserId(string userId);

        Task DeleteByUserIdAsync(string userId);

        void SaveCustomerTypeToNormal(string userId);

        Task SaveCustomerTypeToNormalAsync(string userId);

        List<Customer> GetCustomerServices(string search);

        Task<List<Customer>> GetCustomerServicesAsync(string search);

        Task<List<string>> DeleteCustomersAsync(List<string> userIds, string currentUserId = null);
    }
}