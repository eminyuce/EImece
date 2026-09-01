using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Microsoft.Extensions.Logging;
namespace EImece.Domain.Repositories
{
    public class AddressRepository : BaseEntityRepository<Address>, IAddressRepository
    {
        public AddressRepository(IEImeceContext dbContext, ILogger<AddressRepository> logger) : base(dbContext, logger)
        {
        }
    }
}