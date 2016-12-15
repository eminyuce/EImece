using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;

namespace EImece.Domain.Repositories
{
    public class TagCategoryRepository : BaseRepository<TagCategory, int>, ITagCategoryRepository
    {
        public TagCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int DeleteItem(TagCategory item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public int SaveOrEdit(TagCategory item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }
    }
}
