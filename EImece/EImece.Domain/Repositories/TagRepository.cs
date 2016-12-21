using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;
using EImece.Domain.Models.Enums;
using GenericRepository.EntityFramework.Enums;
using System.Linq.Expressions;
using NLog;

namespace EImece.Domain.Repositories
{
    public class TagRepository : BaseRepository<Tag, int>, ITagRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public TagRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }

   

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
   
        public int DeleteItem(Tag item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }
        public int SaveOrEdit(Tag item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }

         
    }
}
