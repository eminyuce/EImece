using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ISettingRepository : IBaseEntityRepository<Setting>
    {
        List<Setting> GetAllSettings();

        Task<List<Setting>> GetAllSettingsAsync(CancellationToken cancellationToken = default(CancellationToken));

        List<Setting> GetAllActiveSettings();
    }
}
