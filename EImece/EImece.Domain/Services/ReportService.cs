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
            if (startDate == null ||
              endDate == null)
            {
                return null;
            }

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataTable(
                "GetSalesReportByDateRange",
                CommandType.StoredProcedure,
                parameters
            );
        }

        #endregion Sales Report by Date Range

        public DataSet GetPerformanceSystemReport(DateTime startDate, DateTime endDate)
        {
            if (startDate == null ||
                   endDate == null)
            {
                return null;
            }

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetPerformanceSystemReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        public DataSet GetFinancialReport(DateTime startDate, DateTime endDate)
        {
            if (startDate == null ||
              endDate == null)
            {
                return null;
            }

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetFinancialReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        public DataSet GetFraudRiskReport(DateTime startDate, DateTime endDate)
        {
            if (startDate == null ||
              endDate == null)
            {
                return null;
            }

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetFraudRiskReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        public DataSet GetOrderVolumeReport(DateTime startDate, DateTime endDate)
        {
            if (startDate == null ||
              endDate == null)
            {
                return null;
            }

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetOrderVolumeReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        public DataSet GetPaymentTransactionReport(DateTime startDate, DateTime endDate)
        {
            if (startDate == null ||
              endDate == null)
            {
                return null;
            }

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataSet(
                "sp_GetPaymentTransactionReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

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

        #region Product Summary Report

        public DataSet GetProductSummaryReport(DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null, int? productCategoryId = null)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", (object)startDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)endDate ?? DBNull.Value),
                new SqlParameter("@IsActive", (object)isActive ?? DBNull.Value),
                new SqlParameter("@ProductCategoryId", (object)productCategoryId ?? DBNull.Value)
            };

            return DatabaseUtility.ExecuteDataSet(
                "GetProductSummaryReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        #endregion Product Summary Report

        #region Price Analysis Report

        public DataSet GetPriceAnalysisReport(decimal? minPrice = null, decimal? maxPrice = null, int? productCategoryId = null)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@MinPrice", (object)minPrice ?? DBNull.Value),
                new SqlParameter("@MaxPrice", (object)maxPrice ?? DBNull.Value),
                new SqlParameter("@ProductCategoryId", (object)productCategoryId ?? DBNull.Value)
            };

            return DatabaseUtility.ExecuteDataSet(
                "GetPriceAnalysisReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        #endregion Price Analysis Report

        #region Product Inventory Report

        public DataSet GetProductInventoryReport(string state = null, bool? isCampaign = null, bool? mainPage = null)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@State", (object)state ?? DBNull.Value),
                new SqlParameter("@IsCampaign", (object)isCampaign ?? DBNull.Value),
                new SqlParameter("@MainPage", (object)mainPage ?? DBNull.Value)
            };

            return DatabaseUtility.ExecuteDataSet(
                "GetProductInventoryReport",
                CommandType.StoredProcedure,
                parameters
            );
        }

        #endregion Product Inventory Report

        #region Product Details Report

        public DataSet GetProductDetailsReport(int? productId = null, string productCode = null, int? lang = null)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@ProductId", (object)productId ?? DBNull.Value),
                new SqlParameter("@ProductCode", (object)productCode ?? DBNull.Value),
                new SqlParameter("@Lang", (object)lang ?? DBNull.Value)
            };

            return DatabaseUtility.ExecuteDataSet(
                "GetProductDetailsReport",
                CommandType.StoredProcedure,
                parameters
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

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseUtility.ExecuteDataSet(
                "GetProductStatsByDateRange",
                CommandType.StoredProcedure,
                parameters
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