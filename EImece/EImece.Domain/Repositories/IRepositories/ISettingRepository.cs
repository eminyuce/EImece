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

        string GetSettingValue(string key);
        Task<string> GetSettingValueAsync(string key, CancellationToken cancellationToken = default(CancellationToken));
        string GetSettingValue(string key, int language);
        Task<string> GetSettingValueAsync(string key, int language, CancellationToken cancellationToken = default(CancellationToken));
        Dictionary<string, string> GetSettingValues(IEnumerable<string> keys);
        Task<Dictionary<string, string>> GetSettingValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default(CancellationToken));
        List<Models.DTOs.Storefront.SettingKeyValueDto> GetSettingKeyValues(int language);
        Task<List<Models.DTOs.Storefront.SettingKeyValueDto>> GetSettingKeyValuesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}
