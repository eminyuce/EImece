using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;

namespace EImece.Domain.Services
{
    public class AddressService : BaseEntityService<Address>, IAddressService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IAddressRepository AddressRepository { get; set; }

        public AddressService(IAddressRepository repository) : base(repository)
        {
            AddressRepository = repository;
        }
    }
}