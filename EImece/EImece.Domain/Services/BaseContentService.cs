using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
   
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
        public IBaseContentRepository<T> baseContentRepository { get; set; }
        public BaseContentService() : base()
        {
            this.baseEntityRepository = baseContentRepository;
        }

    }
}
