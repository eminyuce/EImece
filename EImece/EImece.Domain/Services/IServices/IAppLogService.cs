using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IAppLogService
    {
        List<AppLog> GetAppLogs(string search, string eventLevel = "");
        Task<List<AppLog>> GetAppLogsAsync(string search, string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken));
        void DeleteAppLogs(List<string> values);
        Task DeleteAppLogsAsync(List<string> values);
        void DeleteAppLog(int id);
        Task DeleteAppLogAsync(int id);
        void RemoveAll(string eventLevel = "");
        Task RemoveAllAsync(string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken));
        int DeleteOldLogs(System.DateTime cutoffDate, int batchSize = 1000);
        Task<int> DeleteOldLogsAsync(System.DateTime cutoffDate, int batchSize = 1000, CancellationToken cancellationToken = default(CancellationToken));
    }
}
