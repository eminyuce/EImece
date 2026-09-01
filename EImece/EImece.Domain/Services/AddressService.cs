using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
namespace EImece.Domain.Services
{
    public class AddressService : BaseEntityService<Address>, IAddressService
    {
        private IAddressRepository AddressRepository { get; set; }

        public AddressService(IAddressRepository repository, ILogger<AddressService> logger) : base(repository, logger)
        {
            AddressRepository = repository;
        }
    }
}