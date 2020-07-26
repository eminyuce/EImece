using EImece.Domain.Entities;
using EImece.Models;

namespace EImece.Domain.Services.IServices
{
    public interface ICustomerService : IBaseEntityService<Customer>
    {
        void SaveRegisterViewModel(RegisterViewModel model);
    }
}