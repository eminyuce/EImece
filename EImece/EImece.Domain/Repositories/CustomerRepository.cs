using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class CustomerRepository : BaseEntityRepository<Customer>, ICustomerRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CustomerRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

       
    }
}