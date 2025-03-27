using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using EImece.Domain.DbContext;
using System.Configuration;

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

        #endregion

        #region Fraud Analysis Report

        public DataTable GetFraudAnalysisReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetFraudAnalysisReport",
                CommandType.StoredProcedure
            );
        }

        #endregion

        #region Payment Method Report

        public DataTable GetPaymentMethodReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetPaymentMethodReport",
                CommandType.StoredProcedure
            );
        }

        #endregion

        #region Payment Status Report

        public DataTable GetPaymentStatusReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetPaymentStatusReport",
                CommandType.StoredProcedure
            );
        }

        #endregion

        #region Regional Sales Report

        public DataTable GetRegionalSalesReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetRegionalSalesReport",
                CommandType.StoredProcedure
            );
        }

        #endregion

        #region Sales Report by Date Range

        public DataTable GetSalesReportByDateRange(DateTime startDate, DateTime endDate)
        {
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

        #endregion

        #region Shipment Company Report

        public DataTable GetShipmentCompanyReport()
        {
            return DatabaseUtility.ExecuteDataTable(
                "GetShipmentCompanyReport",
                CommandType.StoredProcedure
            );
        }

        #endregion

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

        #endregion

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

        #endregion
    }
}