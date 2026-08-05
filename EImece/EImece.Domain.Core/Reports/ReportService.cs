using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EImece.Domain.Core.Reports;

/// <summary>
/// ADO.NET report runner — calls the same SQL Server stored procedures as legacy ReportService.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly string _connectionString;
    private readonly int _commandTimeoutSeconds;

    public ReportService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EImeceDbConnection")
            ?? throw new InvalidOperationException("Connection string 'EImeceDbConnection' is missing.");
        _commandTimeoutSeconds = configuration.GetValue("EImece:DatabaseCommandTimeoutSeconds", 120);
    }

    public Task<DataTable> GetCouponUsageReportAsync(CancellationToken cancellationToken = default)
        => ExecuteDataTableAsync("GetCouponUsageReport", cancellationToken);

    public Task<DataTable> GetFraudAnalysisReportAsync(CancellationToken cancellationToken = default)
        => ExecuteDataTableAsync("GetFraudAnalysisReport", cancellationToken);

    public Task<DataTable> GetPaymentMethodReportAsync(CancellationToken cancellationToken = default)
        => ExecuteDataTableAsync("GetPaymentMethodReport", cancellationToken);

    public Task<DataTable> GetPaymentStatusReportAsync(CancellationToken cancellationToken = default)
        => ExecuteDataTableAsync("GetPaymentStatusReport", cancellationToken);

    public Task<DataTable> GetShipmentCompanyReportAsync(CancellationToken cancellationToken = default)
        => ExecuteDataTableAsync("GetShipmentCompanyReport", cancellationToken);

    public Task<DataTable> GetRegionalSalesReportAsync(string? paymentStatus = null, CancellationToken cancellationToken = default)
    {
        var p = new SqlParameter("@PaymentStatus", SqlDbType.NVarChar)
        {
            Value = string.IsNullOrWhiteSpace(paymentStatus) ? DBNull.Value : paymentStatus.Trim()
        };
        return ExecuteDataTableAsync("GetRegionalSalesReport", cancellationToken, p);
    }

    public Task<DataTable> GetSalesReportByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDataTableAsync(
            "GetSalesReportByDateRange",
            cancellationToken,
            new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate },
            new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });

    public Task<DataSet> GetPriceAnalysisReportAsync(decimal? minPrice = null, decimal? maxPrice = null, int? productCategoryId = null, CancellationToken cancellationToken = default)
        => ExecuteDataSetAsync(
            "GetPriceAnalysisReport",
            cancellationToken,
            NullableDecimal("@MinPrice", minPrice),
            NullableDecimal("@MaxPrice", maxPrice),
            NullableInt("@ProductCategoryId", productCategoryId));

    public Task<DataSet> GetPerformanceSystemReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDateRangeDataSetAsync("sp_GetPerformanceSystemReport", startDate, endDate, cancellationToken);

    public Task<DataSet> GetFinancialReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDateRangeDataSetAsync("sp_GetFinancialReport", startDate, endDate, cancellationToken);

    public Task<DataSet> GetFraudRiskReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDateRangeDataSetAsync("sp_GetFraudRiskReport", startDate, endDate, cancellationToken);

    public Task<DataSet> GetOrderVolumeReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDateRangeDataSetAsync("sp_GetOrderVolumeReport", startDate, endDate, cancellationToken);

    public Task<DataSet> GetPaymentTransactionReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDateRangeDataSetAsync("sp_GetPaymentTransactionReport", startDate, endDate, cancellationToken);

    public Task<DataSet> GetProductSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null, int? productCategoryId = null, CancellationToken cancellationToken = default)
        => ExecuteDataSetAsync(
            "GetProductSummaryReport",
            cancellationToken,
            NullableDateTime("@StartDate", startDate),
            NullableDateTime("@EndDate", endDate),
            NullableBit("@IsActive", isActive),
            NullableInt("@ProductCategoryId", productCategoryId));

    public Task<DataSet> GetProductInventoryReportAsync(string? state = null, bool? isCampaign = null, bool? mainPage = null, CancellationToken cancellationToken = default)
        => ExecuteDataSetAsync(
            "GetProductInventoryReport",
            cancellationToken,
            new SqlParameter("@State", SqlDbType.NVarChar) { Value = (object?)state ?? DBNull.Value },
            NullableBit("@IsCampaign", isCampaign),
            NullableBit("@MainPage", mainPage));

    public Task<DataSet> GetProductStatsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => ExecuteDateRangeDataSetAsync("GetProductStatsByDateRange", startDate, endDate, cancellationToken);

    private Task<DataSet> ExecuteDateRangeDataSetAsync(string procedure, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        => ExecuteDataSetAsync(
            procedure,
            cancellationToken,
            new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate },
            new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });

    private async Task<DataTable> ExecuteDataTableAsync(string procedure, CancellationToken cancellationToken, params SqlParameter[] parameters)
    {
        var set = await ExecuteDataSetAsync(procedure, cancellationToken, parameters).ConfigureAwait(false);
        return set.Tables.Count > 0 ? set.Tables[0] : new DataTable();
    }

    private async Task<DataSet> ExecuteDataSetAsync(string procedure, CancellationToken cancellationToken, params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(procedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeoutSeconds
        };

        if (parameters is { Length: > 0 })
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        var set = new DataSet();
        // SqlDataAdapter.Fill is synchronous; OpenAsync already established the connection.
        using (var adapter = new SqlDataAdapter(command))
        {
            adapter.Fill(set);
        }

        return set;
    }

    private static SqlParameter NullableDecimal(string name, decimal? value)
        => new(name, SqlDbType.Decimal) { Value = value.HasValue ? value.Value : DBNull.Value };

    private static SqlParameter NullableInt(string name, int? value)
        => new(name, SqlDbType.Int) { Value = value.HasValue ? value.Value : DBNull.Value };

    private static SqlParameter NullableDateTime(string name, DateTime? value)
        => new(name, SqlDbType.DateTime) { Value = value.HasValue ? value.Value : DBNull.Value };

    private static SqlParameter NullableBit(string name, bool? value)
        => new(name, SqlDbType.Bit) { Value = value.HasValue ? value.Value : DBNull.Value };
}
