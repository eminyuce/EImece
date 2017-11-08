using GenericRepository;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBrowserSubscriptionRepository : IBaseEntityRepository<BrowserSubscription>
    {

    }
}
