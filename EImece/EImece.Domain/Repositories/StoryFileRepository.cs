using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.GenericRepositories;

namespace EImece.Domain.Repositories
{
    public class StoryFileRepository : BaseRepository<StoryFile, int>, IStoryFileRepository
    {
        public StoryFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public int DeleteItem(StoryFile item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int SaveOrEdit(StoryFile item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }

        public List<StoryFile> GetActiveBaseEntities(bool? isActive)
        {
            return BaseEntityRepository.GetActiveBaseEntities(this, isActive);
        }
    }
}
