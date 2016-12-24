using GenericRepository;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseRepository<T> : IEntityRepository<T, int> where T : class, IEntity<int>
    {
        int SaveOrEdit(T item) ;
        int DeleteItem(T item);
        bool DeleteEntityByWhere(Expression<Func<T, bool>> whereLambda);
        EntitiesContext GetDbContext();
    }
}
