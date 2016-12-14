using GenericRepository;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseRepository<T, TId> : IEntityRepository<T, TId> where T : class, IEntity<TId> where TId : IComparable
    {
        int SaveOrEdit(T item) ;
        int DeleteItem(T item);
    }
}
