using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class SettingRepository : BaseEntityRepository<Setting>, ISettingRepository
    {
        public SettingRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public virtual List<Setting> GetAllActiveSettings()
        {
            return GetAll().Where(t => t.IsActive).ToList();
        }

        public virtual List<Setting> GetAllSettings()
        {
            return GetAll().ToList();
        }

        public virtual async Task<List<Setting>> GetAllSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAll().ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}