using System.Data;

namespace EImece.Domain.Core.Reports;

public interface IReportService
{
    Task<DataTable> GetCouponUsageReportAsync(CancellationToken cancellationToken = default);
    Task<DataTable> GetFraudAnalysisReportAsync(CancellationToken cancellationToken = default);
    Task<DataTable> GetPaymentMethodReportAsync(CancellationToken cancellationToken = default);
    Task<DataTable> GetPaymentStatusReportAsync(CancellationToken cancellationToken = default);
    Task<DataTable> GetRegionalSalesReportAsync(string? paymentStatus = null, CancellationToken cancellationToken = default);
    Task<DataTable> GetSalesReportByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataTable> GetShipmentCompanyReportAsync(CancellationToken cancellationToken = default);
    Task<DataSet> GetPriceAnalysisReportAsync(decimal? minPrice = null, decimal? maxPrice = null, int? productCategoryId = null, CancellationToken cancellationToken = default);
    Task<DataSet> GetPerformanceSystemReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataSet> GetFinancialReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataSet> GetFraudRiskReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataSet> GetOrderVolumeReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataSet> GetPaymentTransactionReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataSet> GetProductSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null, int? productCategoryId = null, CancellationToken cancellationToken = default);
    Task<DataSet> GetProductInventoryReportAsync(string? state = null, bool? isCampaign = null, bool? mainPage = null, CancellationToken cancellationToken = default);
    Task<DataSet> GetProductStatsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
