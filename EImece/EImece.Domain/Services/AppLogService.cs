using EImece.Domain.Entities;
using EImece.Domain.Repositories;
using EImece.Domain.Services.IServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class AppLogService : IAppLogService
    {
        private readonly AppLogRepository _appLogRepository;

        public AppLogService(AppLogRepository appLogRepository)
        {
            _appLogRepository = appLogRepository;
        }

        public List<AppLog> GetAppLogs(string search, string eventLevel = "") => _appLogRepository.GetAppLogs(search, eventLevel);
        public Task<List<AppLog>> GetAppLogsAsync(string search, string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken)) => _appLogRepository.GetAppLogsAsync(search, eventLevel, cancellationToken);
        public void DeleteAppLogs(List<string> values) => _appLogRepository.DeleteAppLogs(values);
        public Task DeleteAppLogsAsync(List<string> values) => _appLogRepository.DeleteAppLogsAsync(values);
        public void DeleteAppLog(int id) => _appLogRepository.DeleteAppLog(id);
        public Task DeleteAppLogAsync(int id) => _appLogRepository.DeleteAppLogAsync(id);
        public void RemoveAll(string eventLevel = "") => _appLogRepository.RemoveAll(eventLevel);
        public Task RemoveAllAsync(string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken)) => _appLogRepository.RemoveAllAsync(eventLevel, cancellationToken);
        public int DeleteOldLogs(System.DateTime cutoffDate, int batchSize = 1000) => _appLogRepository.DeleteOldLogs(cutoffDate, batchSize);
        public Task<int> DeleteOldLogsAsync(System.DateTime cutoffDate, int batchSize = 1000, CancellationToken cancellationToken = default(CancellationToken)) => _appLogRepository.DeleteOldLogsAsync(cutoffDate, batchSize, cancellationToken);
    }
}
