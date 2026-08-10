using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EImece.Domain.Repositories
{
    // AppLog  NLog.config dosyasi uzerinden veritabani kayiti yapilir.
    public class AppLogRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<AppLog> GetAppLogs(string search, string eventLevel = "")
        {
            var applogResult = new List<AppLog>();
            try
            {
                applogResult = GetAppLogsFromDb(search, eventLevel);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
            return applogResult;
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
                parameterList.Add(DatabaseUtility.GetSqlParameter("EventLevel", eventLevel.Trim(), SqlDbType.NVarChar));
            }
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                DatabaseUtility.ExecuteNonQuery(connection, commandText, commandType, parameterList.ToArray());
            }
        }

        public List<AppLog> GetAppLogsFromDb(string search, string eventLevel = "")
        {
            var list = new List<AppLog>();
            var whereClauses = new List<string>();
            var parameterList = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClauses.Add("EventMessage LIKE @Search");
                parameterList.Add(DatabaseUtility.GetSqlParameter("Search", "%" + search.Trim() + "%", SqlDbType.NVarChar));
            }

            if (!string.IsNullOrWhiteSpace(eventLevel))
            {
                whereClauses.Add("LOWER(EventLevel) = LOWER(@EventLevel)");
                parameterList.Add(DatabaseUtility.GetSqlParameter("EventLevel", eventLevel.Trim(), SqlDbType.NVarChar));
            }

            var commandText = whereClauses.Count == 0
                ? @"SELECT TOP 10000 * FROM dbo.AppLogs ORDER BY Id DESC"
                : @"SELECT TOP 10000 * FROM dbo.AppLogs WHERE " + string.Join(" AND ", whereClauses) + " ORDER BY Id DESC";

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