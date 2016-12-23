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
    public class SettingRepository : BaseRepository<Setting, int>, ISettingRepository
    {
        public SettingRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }

        public int DeleteItem(Setting item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<Setting> GetActiveBaseEntities(bool? isActive)
        {
            return BaseEntityRepository.GetActiveBaseEntities(this, isActive);
        }

        public int SaveOrEdit(Setting item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }

       
    }
}
