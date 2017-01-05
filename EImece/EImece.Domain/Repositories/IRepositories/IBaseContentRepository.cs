using EImece.Domain.Entities;
using GenericRepository;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBaseContentRepository<T> : IBaseEntityRepository<T> where T : BaseContent
    {
        List<T> GetActiveBaseContents(bool? isActive, int language);
        T GetBaseContent(int id);


    }
}
