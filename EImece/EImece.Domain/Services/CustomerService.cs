using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;

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

       
    }
}