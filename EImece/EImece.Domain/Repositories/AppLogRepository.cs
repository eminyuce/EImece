using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EImece.Domain.Repositories
{
    // AppLog  NLog.config dosyasi uzerinden veritabani kayiti yapilir.
    public class AppLogRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<AppLog> GetAppLogs(string search)
        {
            var applogResult = new List<AppLog>();
            try
            {
                applogResult = GetAppLogsFromDb(search);
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
            string connectionString = ConfigurationManager.ConnectionStrings[Domain.Constants.DbConnectionKey].ConnectionString;
            String commandText = @"DELETE FROM dbo.AppLogs WHERE Id IN (" + String.Join(",", values) + ")";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                DatabaseUtility.ExecuteNonQuery(connection, commandText, commandType, parameterList.ToArray());
            }
        }

        public void DeleteAppLog(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[Domain.Constants.DbConnectionKey].ConnectionString;
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
            string connectionString = ConfigurationManager.ConnectionStrings[Domain.Constants.DbConnectionKey].ConnectionString;
            String commandText = "";
            if (string.IsNullOrEmpty(eventLevel))
            {
                commandText = @"DELETE FROM dbo.AppLogs";
            }
            else
            {
                commandText = @"DELETE FROM dbo.AppLogs where EventLevel=@EventLevel";
            }
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("EventLevel", eventLevel, SqlDbType.NVarChar));
            var commandType = CommandType.Text;
            using (var connection = new SqlConnection(connectionString))
            {
                DatabaseUtility.ExecuteNonQuery(connection, commandText, commandType, parameterList.ToArray());
            }
        }

        public List<AppLog> GetAppLogsFromDb(string search)
        {
            var list = new List<AppLog>();
            String commandText = "";
            if (string.IsNullOrEmpty(search))
            {
                commandText = @"SELECT * FROM dbo.AppLogs ORDER BY Id DESC";
            }
            else
            {
                commandText = @"SELECT * FROM dbo.AppLogs where EventMessage LIKE '%" + search.Trim() + "%' ORDER BY Id DESC";
            }

            var parameterList = new List<SqlParameter>();
            string connectionString = ConfigurationManager.ConnectionStrings[Domain.Constants.DbConnectionKey].ConnectionString;
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