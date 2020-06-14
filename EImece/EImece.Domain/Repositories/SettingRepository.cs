using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class SettingRepository : BaseEntityRepository<Setting>, ISettingRepository
    {
        public SettingRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Setting> GetAllActiveSettings()
        {
            return GetAll().Where(t => t.IsActive).ToList();
        }

        public List<Setting> GetAllSettings()
        {
            return GetAll().ToList();
        }
    }
}