using Microsoft.Extensions.Logging;
﻿using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
namespace EImece.Domain.Repositories
{
    public class AddressRepository : BaseEntityRepository<Address>, IAddressRepository
    {
        public AddressRepository(IEImeceContext dbContext, ILogger<AddressRepository> logger) : base(dbContext, logger) {
        }
    }
}