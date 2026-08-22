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
            return GetAllReadOnly().ToList();
        }

        public virtual async Task<List<Setting>> GetAllSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAllReadOnly().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        // Minimal projection — single column, no entity materialization
        public virtual string GetSettingValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return EImeceDbContext.Settings.AsNoTracking()
                .Where(s => s.SettingKey == key)
                .OrderByDescending(s => s.UpdatedDate).ThenByDescending(s => s.Id)
                .Select(s => s.SettingValue)
                .FirstOrDefault();
        }

        public virtual async Task<string> GetSettingValueAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return await EImeceDbContext.Settings.AsNoTracking()
                .Where(s => s.SettingKey == key)
                .OrderByDescending(s => s.UpdatedDate).ThenByDescending(s => s.Id)
                .Select(s => s.SettingValue)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual string GetSettingValue(string key, int language)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return EImeceDbContext.Settings.AsNoTracking()
                .Where(s => s.SettingKey == key && s.Lang == language)
                .OrderByDescending(s => s.UpdatedDate).ThenByDescending(s => s.Id)
                .Select(s => s.SettingValue)
                .FirstOrDefault();
        }

        public virtual async Task<string> GetSettingValueAsync(string key, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return await EImeceDbContext.Settings.AsNoTracking()
                .Where(s => s.SettingKey == key && s.Lang == language)
                .OrderByDescending(s => s.UpdatedDate).ThenByDescending(s => s.Id)
                .Select(s => s.SettingValue)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual Dictionary<string, string> GetSettingValues(IEnumerable<string> keys)
        {
            if (keys == null) return new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            var keyList = keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(System.StringComparer.OrdinalIgnoreCase).ToList();
            if (!keyList.Any()) return new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            return EImeceDbContext.Settings.AsNoTracking()
                .Where(s => keyList.Contains(s.SettingKey))
                .GroupBy(s => s.SettingKey, System.StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.Id).FirstOrDefault())
                .Where(s => s != null)
                .ToDictionary(s => s.SettingKey, s => s.SettingValue, System.StringComparer.OrdinalIgnoreCase);
        }

        public virtual async Task<Dictionary<string, string>> GetSettingValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null) return new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            var keyList = keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(System.StringComparer.OrdinalIgnoreCase).ToList();
            if (!keyList.Any()) return new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            var rows = await EImeceDbContext.Settings.AsNoTracking()
                .Where(s => keyList.Contains(s.SettingKey))
                .GroupBy(s => s.SettingKey)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.Id).FirstOrDefault())
                .Where(s => s != null)
                .Select(s => new { s.SettingKey, s.SettingValue })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            return rows.ToDictionary(x => x.SettingKey, x => x.SettingValue, System.StringComparer.OrdinalIgnoreCase);
        }

        public virtual List<Models.DTOs.Storefront.SettingKeyValueDto> GetSettingKeyValues(int language)
        {
            return EImeceDbContext.Settings.AsNoTracking()
                .Where(s => s.Lang == language && s.IsActive)
                .Select(s => new Models.DTOs.Storefront.SettingKeyValueDto
                {
                    SettingKey = s.SettingKey,
                    SettingValue = s.SettingValue
                }).ToList();
        }

        public virtual async Task<List<Models.DTOs.Storefront.SettingKeyValueDto>> GetSettingKeyValuesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Settings.AsNoTracking()
                .Where(s => s.Lang == language && s.IsActive)
                .Select(s => new Models.DTOs.Storefront.SettingKeyValueDto
                {
                    SettingKey = s.SettingKey,
                    SettingValue = s.SettingValue
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}