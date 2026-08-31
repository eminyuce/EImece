using Microsoft.Extensions.Logging;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    // AppLog  NLog.config dosyasi uzerinden veritabani kayiti yapilir.
    public class AppLogRepository
    {
        private readonly ILogger<AppLogRepository> _logger;

        public AppLogRepository(ILogger<AppLogRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<AppLog> GetAppLogs(string search, string eventLevel = "")
        {
            var applogResult = new List<AppLog>();
            try
            {
                applogResult = GetAppLogsFromDb(search, eventLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            return applogResult;
        }

        public async Task<List<AppLog>> GetAppLogsAsync(string search, string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await GetAppLogsFromDbAsync(search, eventLevel, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAppLogsAsync failed.");
                throw new InvalidOperationException("GetAppLogsAsync failed.", ex);
            }
        }

        public void DeleteAppLogs(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            var ids = new List<int>();
            foreach (var value in values)
            {
                if (!int.TryParse(value, out var id) || id <= 0)
                {
                    throw new ArgumentException("Invalid log id: " + value, nameof(values));
                }

                ids.Add(id);
            }

            string connectionString = ConnectionStringProvider.GetConnectionString();
            var parameterList = new List<SqlParameter>();
            var parameterNames = new List<string>();
            for (var i = 0; i < ids.Count; i++)
            {
                var parameterName = "Id" + i;
                parameterNames.Add("@" + parameterName);
                parameterList.Add(DatabaseUtility.GetSqlParameter(parameterName, ids[i], SqlDbType.Int));
            }

            String commandText = @"DELETE FROM dbo.AppLogs WHERE Id IN (" + String.Join(",", parameterNames) + ")";
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                DatabaseUtility.ExecuteNonQuery(connection, commandText, commandType, parameterList.ToArray());
            }
        }

        public async Task DeleteAppLogsAsync(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            var ids = new List<int>();
            foreach (var value in values)
            {
                if (!int.TryParse(value, out var id) || id <= 0)
                {
                    throw new ArgumentException("Invalid log id: " + value, nameof(values));
                }

                ids.Add(id);
            }

            string connectionString = ConnectionStringProvider.GetConnectionString();
            var parameterList = new List<SqlParameter>();
            var parameterNames = new List<string>();
            for (var i = 0; i < ids.Count; i++)
            {
                var parameterName = "Id" + i;
                parameterNames.Add("@" + parameterName);
                parameterList.Add(DatabaseUtility.GetSqlParameter(parameterName, ids[i], SqlDbType.Int));
            }

            String commandText = @"DELETE FROM dbo.AppLogs WHERE Id IN (" + String.Join(",", parameterNames) + ")";
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                await ExecuteNonQueryAsync(connection, commandText, commandType, parameterList.ToArray()).ConfigureAwait(false);
            }
        }

        public void DeleteAppLog(int id)
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            String commandText = @"DELETE FROM dbo.AppLogs WHERE Id=@Id";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.Text;
            parameterList.Add(DatabaseUtility.GetSqlParameter("Id", id, SqlDbType.Int));
            using (var connection = new SqlConnection(connectionString))
            {
                DatabaseUtility.ExecuteNonQuery(connection, commandText, commandType, parameterList.ToArray());
            }
        }

        public async Task DeleteAppLogAsync(int id)
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            String commandText = @"DELETE FROM dbo.AppLogs WHERE Id=@Id";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.Text;
            parameterList.Add(DatabaseUtility.GetSqlParameter("Id", id, SqlDbType.Int));
            using (var connection = new SqlConnection(connectionString))
            {
                await ExecuteNonQueryAsync(connection, commandText, commandType, parameterList.ToArray()).ConfigureAwait(false);
            }
        }

        public void RemoveAll(string eventLevel = "")
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            String commandText;
            var parameterList = new List<SqlParameter>();
            if (string.IsNullOrEmpty(eventLevel))
            {
                commandText = @"DELETE FROM dbo.AppLogs";
            }
            else
            {
                commandText = @"DELETE FROM dbo.AppLogs WHERE LOWER(EventLevel) = LOWER(@EventLevel)";
                parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EventLevelColumn, eventLevel.Trim(), SqlDbType.NVarChar));
            }
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                DatabaseUtility.ExecuteNonQuery(connection, commandText, commandType, parameterList.ToArray());
            }
        }

        public async Task RemoveAllAsync(string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            String commandText;
            var parameterList = new List<SqlParameter>();
            if (string.IsNullOrEmpty(eventLevel))
            {
                commandText = @"DELETE FROM dbo.AppLogs";
            }
            else
            {
                commandText = @"DELETE FROM dbo.AppLogs WHERE LOWER(EventLevel) = LOWER(@EventLevel)";
                parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EventLevelColumn, eventLevel.Trim(), SqlDbType.NVarChar));
            }
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                await ExecuteNonQueryAsync(connection, commandText, commandType, parameterList.ToArray(), cancellationToken).ConfigureAwait(false);
            }
        }

        public List<AppLog> GetAppLogsFromDb(string search, string eventLevel = "")
        {
            var list = new List<AppLog>();
            var parameterList = new List<SqlParameter>();
            var commandText = BuildAppLogsCommandText(search, eventLevel, parameterList);

            string connectionString = ConnectionStringProvider.GetConnectionString();
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = DatabaseUtility.ExecuteDataSet(connection, commandText, commandType, parameterList.ToArray());
                if (dataSet.Tables.Count > 0)
                {
                    using (DataTable dt = dataSet.Tables[0])
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var e = GetAppLogFromDataRow(dr);
                            list.Add(e);
                        }
                    }
                }
                dataSet.Dispose();
            }
            return list;
        }

        public async Task<List<AppLog>> GetAppLogsFromDbAsync(string search, string eventLevel = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AppLog>();
            var parameterList = new List<SqlParameter>();
            var commandText = BuildAppLogsCommandText(search, eventLevel, parameterList);

            string connectionString = ConnectionStringProvider.GetConnectionString();
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;
                    if (parameterList.Count > 0)
                    {
                        command.Parameters.AddRange(parameterList.ToArray());
                    }
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            var e = new AppLog();
                            e.Id = reader["Id"].ToInt();
                            e.EventDateTime = reader["EventDateTime"].ToStr();
                            e.EventLevel = reader["EventLevel"].ToStr();
                            e.UserName = reader["UserName"].ToStr();
                            e.MachineName = reader["MachineName"].ToStr();
                            e.EventMessage = reader["EventMessage"].ToStr();
                            e.ErrorSource = reader["ErrorSource"].ToStr();
                            e.ErrorClass = reader["ErrorClass"].ToStr();
                            e.ErrorMethod = reader["ErrorMethod"].ToStr();
                            e.ErrorMessage = reader["ErrorMessage"].ToStr();
                            e.InnerErrorMessage = reader["InnerErrorMessage"].ToStr();
                            e.CreatedDate = reader["CreatedDate"].ToDateTime();
                            list.Add(e);
                        }
                    }
                }
            }
            return list;
        }

        private static string BuildAppLogsCommandText(string search, string eventLevel, List<SqlParameter> parameterList)
        {
            var whereClauses = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClauses.Add("EventMessage LIKE @Search");
                parameterList.Add(DatabaseUtility.GetSqlParameter("Search", "%" + search.Trim() + "%", SqlDbType.NVarChar));
            }

            if (!string.IsNullOrWhiteSpace(eventLevel))
            {
                whereClauses.Add("LOWER(EventLevel) = LOWER(@EventLevel)");
                parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EventLevelColumn, eventLevel.Trim(), SqlDbType.NVarChar));
            }

            return whereClauses.Count == 0
                ? @"SELECT TOP 10000 * FROM dbo.AppLogs ORDER BY Id DESC"
                : @"SELECT TOP 10000 * FROM dbo.AppLogs WHERE " + string.Join(" AND ", whereClauses) + " ORDER BY Id DESC";
        }

        private static async Task ExecuteNonQueryAsync(SqlConnection connection, string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public int DeleteOldLogs(DateTime cutoffDate, int batchSize = 1000)
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            int totalDeleted = 0;
            string commandText = @"DELETE TOP (@BatchSize) FROM dbo.AppLogs WHERE CreatedDate < @CutoffDate";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                while (true)
                {
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("BatchSize", batchSize, SqlDbType.Int));
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("CutoffDate", cutoffDate, SqlDbType.DateTime));
                        int rows = command.ExecuteNonQuery();
                        totalDeleted += rows;
                        if (rows < batchSize)
                        {
                            break;
                        }
                    }
                }
            }
            return totalDeleted;
        }

        public async Task<int> DeleteOldLogsAsync(DateTime cutoffDate, int batchSize = 1000, CancellationToken cancellationToken = default(CancellationToken))
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            int totalDeleted = 0;
            string commandText = @"DELETE TOP (@BatchSize) FROM dbo.AppLogs WHERE CreatedDate < @CutoffDate";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("BatchSize", batchSize, SqlDbType.Int));
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("CutoffDate", cutoffDate, SqlDbType.DateTime));
                        int rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        totalDeleted += rows;
                        if (rows < batchSize)
                        {
                            break;
                        }
                    }
                }
            }
            return totalDeleted;
        }

        private static AppLog GetAppLogFromDataRow(DataRow dr)
        {
            var item = new AppLog();

            item.Id = dr["Id"].ToInt();
            item.EventDateTime = dr["EventDateTime"].ToStr();
            item.EventLevel = dr["EventLevel"].ToStr();
            item.UserName = dr["UserName"].ToStr();
            item.MachineName = dr["MachineName"].ToStr();
            item.EventMessage = dr["EventMessage"].ToStr();
            item.ErrorSource = dr["ErrorSource"].ToStr();
            item.ErrorClass = dr["ErrorClass"].ToStr();
            item.ErrorMethod = dr["ErrorMethod"].ToStr();
            item.ErrorMessage = dr["ErrorMessage"].ToStr();
            item.InnerErrorMessage = dr["InnerErrorMessage"].ToStr();
            item.CreatedDate = dr["CreatedDate"].ToDateTime();
            return item;
        }
    }
}