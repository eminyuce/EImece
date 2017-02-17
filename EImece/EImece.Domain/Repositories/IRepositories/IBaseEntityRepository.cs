using EImece.Domain.Entities;
using GenericRepository;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseEntityRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        List<T> GetActiveBaseEntities(bool? isActive, int language);
        List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language);

    }
}
