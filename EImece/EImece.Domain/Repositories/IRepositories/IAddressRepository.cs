using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IAddressRepository : IBaseEntityRepository<Address>, IDisposable
    {
    }
}