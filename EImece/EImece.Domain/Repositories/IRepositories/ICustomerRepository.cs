﻿using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ICustomerRepository : IBaseEntityRepository<Customer>, IDisposable
    {

    }
}