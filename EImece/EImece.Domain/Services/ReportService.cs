using EImece.Domain.DbContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EImece.Domain.Services
{
    public class ReportService : IDisposable
    {
        private readonly SqlConnection _connection;
        private bool _disposed = false;

        public ReportService() : this(GetConnectionStringFromConfig())
        {
        }

        // Helper method to get connection string from configuration
        private static string GetConnectionStringFromConfig()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["EImeceDbConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationErrorsException("Connection string not found in configuration");

            return connectionString;
        }

        private ReportService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            _connection = new SqlConnection(connectionString);
            DatabaseUtility.Connection = _connection;
        }

        #region Coupon Usage Report

        public DataTable GetCouponUsageReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetCouponUsageReport",
                CommandType.StoredProcedure
            );
        }

        #endregion Coupon Usage Report

        #region Fraud Analysis Report

        public DataTable GetFraudAnalysisReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetFraudAnalysisReport",
                CommandType.StoredProcedure
            );
        }

        #endregion Fraud Analysis Report

        #region Payment Method Report

        public DataTable GetPaymentMethodReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetPaymentMethodReport",
                CommandType.StoredProcedure
            );
        }

        #endregion Payment Method Report

        #region Payment Status Report

        public DataTable GetPaymentStatusReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetPaymentStatusReport",
                CommandType.StoredProcedure
            );
        }

        #endregion Payment Status Report

        #region Regional Sales Report

        public DataTable GetRegionalSalesReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetRegionalSalesReport",
                CommandType.StoredProcedure
            );
        }

        #endregion Regional Sales Report

        #region Sales Report by Date Range

        public DataTable GetSalesReportByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate == null || endDate == null)
            {
                return null;
            }

            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("StartDate", startDate, SqlDbType.DateTime));
            parameterList.Add(DatabaseUtility.GetSqlParameter("EndDate", endDate, SqlDbType.DateTime));

            return DatabaseUtility.ExecuteDataTable(
                "GetSalesReportByDateRange",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        #endregion Sales Report by Date Range

        #region Shipment Company Report

        public DataTable GetShipmentCompanyReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetShipmentCompanyReport",
                CommandType.StoredProcedure
            );
        }

        #endregion Shipment Company Report

        #region Helper Methods

        public List<Dictionary<string, object>> ConvertDataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return list;
        }

        #endregion Helper Methods

        #region Price Analysis Report

        public DataSet GetPriceAnalysisReport(decimal? minPrice = null, decimal? maxPrice = null, int? productCategoryId = null)
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter("@MinPrice", minPrice, SqlDbType.Decimal),
                DatabaseUtility.GetSqlParameter("@MaxPrice", maxPrice, SqlDbType.Decimal),
                DatabaseUtility.GetSqlParameter("@ProductCategoryId", productCategoryId, SqlDbType.Int)
            };

            return DatabaseUtility.ExecuteDataSet(
                "GetPriceAnalysisReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        public DataSet GetPerformanceSystemReport(DateTime startDate, DateTime endDate)
        {
            var parameterList = new List<SqlParameter>
                {
                    DatabaseUtility.GetSqlParameter("@StartDate", startDate, SqlDbType.DateTime),
                    DatabaseUtility.GetSqlParameter("@EndDate", endDate, SqlDbType.DateTime)
                };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetPerformanceSystemReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        public DataSet GetFinancialReport(DateTime startDate, DateTime endDate)
        {
            var parameterList = new List<SqlParameter>
    {
        DatabaseUtility.GetSqlParameter("@StartDate", startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@EndDate", endDate, SqlDbType.DateTime)
    };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetFinancialReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        public DataSet GetFraudRiskReport(DateTime startDate, DateTime endDate)
        {
            var parameterList = new List<SqlParameter>
    {
        DatabaseUtility.GetSqlParameter("@StartDate", startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@EndDate", endDate, SqlDbType.DateTime)
    };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetFraudRiskReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        public DataSet GetOrderVolumeReport(DateTime startDate, DateTime endDate)
        {
            var parameterList = new List<SqlParameter>
    {
        DatabaseUtility.GetSqlParameter("@StartDate", startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@EndDate", endDate, SqlDbType.DateTime)
    };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetOrderVolumeReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        public DataSet GetPaymentTransactionReport(DateTime startDate, DateTime endDate)
        {
            var parameterList = new List<SqlParameter>
    {
        DatabaseUtility.GetSqlParameter("@StartDate", startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@EndDate", endDate, SqlDbType.DateTime)
    };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetPaymentTransactionReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        public DataSet GetProductSummaryReport(DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null, int? productCategoryId = null)
        {
            var parameterList = new List<SqlParameter>
    {
        DatabaseUtility.GetSqlParameter("@StartDate", startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@EndDate", endDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@IsActive", isActive, SqlDbType.Bit),
        DatabaseUtility.GetSqlParameter("@ProductCategoryId", productCategoryId, SqlDbType.Int)
    };

            return DatabaseUtility.ExecuteDataSet(
                "GetProductSummaryReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        #endregion Price Analysis Report

        #region Product Inventory Report

        public DataSet GetProductInventoryReport(string state = null, bool? isCampaign = null, bool? mainPage = null)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("State", state, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("IsCampaign", isCampaign, SqlDbType.Bit));
            parameterList.Add(DatabaseUtility.GetSqlParameter("MainPage", mainPage, SqlDbType.Bit));

            return DatabaseUtility.ExecuteDataSet(
                "GetProductInventoryReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        #endregion Product Inventory Report

        #region Product Details Report

        public DataSet GetProductDetailsReport(int? productId = null, string productCode = null, int? lang = null)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductId", productId, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductCode", productCode, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("Lang", lang, SqlDbType.Int));

            return DatabaseUtility.ExecuteDataSet(
                "GetProductDetailsReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        #endregion Product Details Report

        #region Product Stats by Date Range

        public DataSet GetProductStatsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate == null || endDate == null)
            {
                return null;
            }

            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("StartDate", startDate, SqlDbType.DateTime));
            parameterList.Add(DatabaseUtility.GetSqlParameter("EndDate", endDate, SqlDbType.DateTime));

            return DatabaseUtility.ExecuteDataSet(
                "GetProductStatsByDateRange",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        #endregion Product Stats by Date Range

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        DatabaseUtility.Connection = null;
                    }
                }
                _disposed = true;
            }
        }

        ~ReportService()
        {
            Dispose(false);
        }

        #endregion IDisposable Implementation
    }
}