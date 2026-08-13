using EImece.Domain.DbContext;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ReportService : IDisposable
    {
        private readonly SqlConnection _connection;
        private bool _disposed = false;

        public ReportService() : this(GetConnectionStringFromConfig())
        {
        }

        // Helper method to get connection string from configuration / environment
        private static string GetConnectionStringFromConfig()
        {
            return ConnectionStringProvider.GetConnectionString();
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

        /// <summary>
        /// Regional sales by city. Pass paymentStatus to filter (e.g. SUCCESS); null/empty = all statuses.
        /// </summary>
        public DataTable GetRegionalSalesReport(string paymentStatus = null)
        {
            object statusValue = string.IsNullOrWhiteSpace(paymentStatus)
                ? (object)DBNull.Value
                : paymentStatus.Trim();

            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter("@PaymentStatus", statusValue, SqlDbType.NVarChar)
            };

            DatabaseUtility.Connection = _connection;
            return DatabaseUtility.ExecuteDataTable(
                "GetRegionalSalesReport",
                CommandType.StoredProcedure,
                parameterList.ToArray()
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
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.StartDateParam, startDate, SqlDbType.DateTime));
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EndDateParam, endDate, SqlDbType.DateTime));

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
                DatabaseUtility.GetSqlParameter("@MinPrice", minPrice, SqlDbType.Money),
                DatabaseUtility.GetSqlParameter("@MaxPrice", maxPrice, SqlDbType.Money),
                DatabaseUtility.GetSqlParameter(Constants.ProductCategoryIdSqlParam, productCategoryId, SqlDbType.Int)
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
                    DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                    DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
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
        DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
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
        DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
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
        DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
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
        DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
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
        DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime),
        DatabaseUtility.GetSqlParameter("@IsActive", isActive, SqlDbType.Bit),
        DatabaseUtility.GetSqlParameter(Constants.ProductCategoryIdSqlParam, productCategoryId, SqlDbType.Int)
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
            parameterList.Add(DatabaseUtility.GetSqlParameter("@State", state, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("@IsCampaign", isCampaign, SqlDbType.Bit));
            parameterList.Add(DatabaseUtility.GetSqlParameter("@MainPage", mainPage, SqlDbType.Bit));

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
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.StartDateParam, startDate, SqlDbType.DateTime));
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EndDateParam, endDate, SqlDbType.DateTime));

            return DatabaseUtility.ExecuteDataSet(
                "GetProductStatsByDateRange",
                CommandType.StoredProcedure,
                parameterList.ToArray()
            );
        }

        #endregion Product Stats by Date Range

        #region Async Report Methods

        public Task<DataTable> GetCouponUsageReportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteDataTableStoredProcAsync("GetCouponUsageReport", null, cancellationToken);
        }

        public Task<DataTable> GetFraudAnalysisReportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteDataTableStoredProcAsync("GetFraudAnalysisReport", null, cancellationToken);
        }

        public Task<DataTable> GetPaymentMethodReportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteDataTableStoredProcAsync("GetPaymentMethodReport", null, cancellationToken);
        }

        public Task<DataTable> GetPaymentStatusReportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteDataTableStoredProcAsync("GetPaymentStatusReport", null, cancellationToken);
        }

        public Task<DataTable> GetRegionalSalesReportAsync(string paymentStatus = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            object statusValue = string.IsNullOrWhiteSpace(paymentStatus)
                ? (object)DBNull.Value
                : paymentStatus.Trim();
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter("@PaymentStatus", statusValue, SqlDbType.NVarChar)
            };
            return ExecuteDataTableStoredProcAsync("GetRegionalSalesReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataTable> GetSalesReportByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (startDate == null || endDate == null)
            {
                return Task.FromResult<DataTable>(null);
            }
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.StartDateParam, startDate, SqlDbType.DateTime));
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EndDateParam, endDate, SqlDbType.DateTime));
            return ExecuteDataTableStoredProcAsync("GetSalesReportByDateRange", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataTable> GetShipmentCompanyReportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteDataTableStoredProcAsync("GetShipmentCompanyReport", null, cancellationToken);
        }

        public Task<DataSet> GetPerformanceSystemReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
            };
            return ExecuteDataSetStoredProcAsync("sp_GetPerformanceSystemReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetFinancialReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
            };
            return ExecuteDataSetStoredProcAsync("sp_GetFinancialReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetFraudRiskReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
            };
            return ExecuteDataSetStoredProcAsync("sp_GetFraudRiskReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetOrderVolumeReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
            };
            return ExecuteDataSetStoredProcAsync("sp_GetOrderVolumeReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetPaymentTransactionReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime)
            };
            return ExecuteDataSetStoredProcAsync("sp_GetPaymentTransactionReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetProductSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null, int? productCategoryId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter(Constants.StartDateSqlParam, startDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter(Constants.EndDateSqlParam, endDate, SqlDbType.DateTime),
                DatabaseUtility.GetSqlParameter("@IsActive", isActive, SqlDbType.Bit),
                DatabaseUtility.GetSqlParameter(Constants.ProductCategoryIdSqlParam, productCategoryId, SqlDbType.Int)
            };
            return ExecuteDataSetStoredProcAsync("GetProductSummaryReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetPriceAnalysisReportAsync(decimal? minPrice = null, decimal? maxPrice = null, int? productCategoryId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>
            {
                DatabaseUtility.GetSqlParameter("@MinPrice", minPrice, SqlDbType.Money),
                DatabaseUtility.GetSqlParameter("@MaxPrice", maxPrice, SqlDbType.Money),
                DatabaseUtility.GetSqlParameter(Constants.ProductCategoryIdSqlParam, productCategoryId, SqlDbType.Int)
            };
            return ExecuteDataSetStoredProcAsync("GetPriceAnalysisReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetProductInventoryReportAsync(string state = null, bool? isCampaign = null, bool? mainPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("@State", state, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("@IsCampaign", isCampaign, SqlDbType.Bit));
            parameterList.Add(DatabaseUtility.GetSqlParameter("@MainPage", mainPage, SqlDbType.Bit));
            return ExecuteDataSetStoredProcAsync("GetProductInventoryReport", parameterList.ToArray(), cancellationToken);
        }

        public Task<DataSet> GetProductStatsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (startDate == null || endDate == null)
            {
                return Task.FromResult<DataSet>(null);
            }
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.StartDateParam, startDate, SqlDbType.DateTime));
            parameterList.Add(DatabaseUtility.GetSqlParameter(Constants.EndDateParam, endDate, SqlDbType.DateTime));
            return ExecuteDataSetStoredProcAsync("GetProductStatsByDateRange", parameterList.ToArray(), cancellationToken);
        }

        private async Task<DataTable> ExecuteDataTableStoredProcAsync(string commandText, SqlParameter[] parameters, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(_connection.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
        }

        private async Task<DataSet> ExecuteDataSetStoredProcAsync(string commandText, SqlParameter[] parameters, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(_connection.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataSet = new DataSet();
                        adapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
        }

        #endregion Async Report Methods

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