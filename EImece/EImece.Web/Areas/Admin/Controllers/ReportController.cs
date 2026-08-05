using System.Data;
using EImece.Domain.Core.Reports;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class ReportController : BaseAdminController
{
    private readonly IReportService _reports;
    private readonly IReportExportService _export;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IOptions<EImeceOptions> siteOptions,
        IReportService reports,
        IReportExportService export,
        ILogger<ReportController> logger)
        : base(siteOptions)
    {
        _reports = reports;
        _export = export;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public Task<IActionResult> CouponUsage(CancellationToken ct)
        => TableReportAsync("Kupon Kullanımı", nameof(CouponUsage), () => _reports.GetCouponUsageReportAsync(ct), ct);

    [HttpGet]
    public Task<IActionResult> FraudAnalysis(CancellationToken ct)
        => TableReportAsync("Dolandırıcılık Analizi", nameof(FraudAnalysis), () => _reports.GetFraudAnalysisReportAsync(ct), ct);

    [HttpGet]
    public Task<IActionResult> PaymentMethod(CancellationToken ct)
        => TableReportAsync("Ödeme Yöntemleri", nameof(PaymentMethod), () => _reports.GetPaymentMethodReportAsync(ct), ct);

    [HttpGet]
    public Task<IActionResult> PaymentStatus(CancellationToken ct)
        => TableReportAsync("Ödeme Durumları", nameof(PaymentStatus), () => _reports.GetPaymentStatusReportAsync(ct), ct);

    [HttpGet]
    public Task<IActionResult> GetRegionalSalesReport(string paymentStatus = "SUCCESS", CancellationToken ct = default)
        => TableReportAsync("Bölgesel Satışlar", nameof(GetRegionalSalesReport), () => _reports.GetRegionalSalesReportAsync(paymentStatus, ct), ct);

    [HttpGet]
    public async Task<IActionResult> SalesByDateRange(DateTime? startDate, DateTime? endDate, CancellationToken ct)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today;
        var model = await BuildTableModelAsync("Tarihe Göre Satışlar", nameof(SalesByDateRange),
            () => _reports.GetSalesReportByDateRangeAsync(start, end, ct), ct).ConfigureAwait(false);
        model.StartDate = start;
        model.EndDate = end;
        return View("ReportResult", model);
    }

    [HttpGet]
    public Task<IActionResult> ShipmentCompany(CancellationToken ct)
        => TableReportAsync("Kargo Firmaları", nameof(ShipmentCompany), () => _reports.GetShipmentCompanyReportAsync(ct), ct);

    [HttpGet]
    public IActionResult PerformanceSystemReport() => DateFilterView("Sistem Performansı", nameof(PerformanceSystemReport));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> PerformanceSystemReport(DateTime startDate, DateTime endDate, CancellationToken ct)
        => SetReportAsync("Sistem Performansı", nameof(PerformanceSystemReport), () => _reports.GetPerformanceSystemReportAsync(startDate, endDate, ct), startDate, endDate, ct);

    [HttpGet]
    public IActionResult FinancialReport() => DateFilterView("Finansal Özet", nameof(FinancialReport));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> FinancialReport(DateTime startDate, DateTime endDate, CancellationToken ct)
        => SetReportAsync("Finansal Özet", nameof(FinancialReport), () => _reports.GetFinancialReportAsync(startDate, endDate, ct), startDate, endDate, ct);

    [HttpGet]
    public IActionResult FraudRiskReport() => DateFilterView("Dolandırıcılık Riski", nameof(FraudRiskReport));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> FraudRiskReport(DateTime startDate, DateTime endDate, CancellationToken ct)
        => SetReportAsync("Dolandırıcılık Riski", nameof(FraudRiskReport), () => _reports.GetFraudRiskReportAsync(startDate, endDate, ct), startDate, endDate, ct);

    [HttpGet]
    public IActionResult OrderVolumeReport() => DateFilterView("Sipariş Hacmi", nameof(OrderVolumeReport));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> OrderVolumeReport(DateTime startDate, DateTime endDate, CancellationToken ct)
        => SetReportAsync("Sipariş Hacmi", nameof(OrderVolumeReport), () => _reports.GetOrderVolumeReportAsync(startDate, endDate, ct), startDate, endDate, ct);

    [HttpGet]
    public IActionResult PaymentTransactionReport() => DateFilterView("Ödeme İşlemleri", nameof(PaymentTransactionReport));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> PaymentTransactionReport(DateTime startDate, DateTime endDate, CancellationToken ct)
        => SetReportAsync("Ödeme İşlemleri", nameof(PaymentTransactionReport), () => _reports.GetPaymentTransactionReportAsync(startDate, endDate, ct), startDate, endDate, ct);

    [HttpGet]
    public async Task<IActionResult> ProductSummary(DateTime? startDate, DateTime? endDate, bool? isActive, int? productCategoryId, CancellationToken ct)
    {
        var model = await BuildSetModelAsync("Ürün Özeti", nameof(ProductSummary),
            () => _reports.GetProductSummaryReportAsync(startDate, endDate, isActive, productCategoryId, ct), ct).ConfigureAwait(false);
        model.StartDate = startDate;
        model.EndDate = endDate;
        return View("ReportResult", model);
    }

    [HttpGet]
    public async Task<IActionResult> PriceAnalysis(decimal? minPrice, decimal? maxPrice, int? productCategoryId, CancellationToken ct)
    {
        var model = await BuildSetModelAsync("Fiyat Analizi", nameof(PriceAnalysis),
            () => _reports.GetPriceAnalysisReportAsync(minPrice, maxPrice, productCategoryId, ct), ct).ConfigureAwait(false);
        return View("ReportResult", model);
    }

    [HttpGet]
    public async Task<IActionResult> ProductInventory(string? state, bool? isCampaign, bool? mainPage, CancellationToken ct)
    {
        var model = await BuildSetModelAsync("Stok Durumu", nameof(ProductInventory),
            () => _reports.GetProductInventoryReportAsync(state, isCampaign, mainPage, ct), ct).ConfigureAwait(false);
        return View("ReportResult", model);
    }

    [HttpGet]
    public IActionResult ProductStatsByDateRange() => DateFilterView("Ürün İstatistikleri", nameof(ProductStatsByDateRange));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ProductStatsByDateRange(DateTime startDate, DateTime endDate, CancellationToken ct)
        => SetReportAsync("Ürün İstatistikleri", nameof(ProductStatsByDateRange), () => _reports.GetProductStatsByDateRangeAsync(startDate, endDate, ct), startDate, endDate, ct);

    [HttpGet]
    public async Task<IActionResult> Export(string actionName, string format = "xlsx", DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;
            DataTable? table = null;
            DataSet? set = null;

            switch (actionName)
            {
                case nameof(CouponUsage): table = await _reports.GetCouponUsageReportAsync(ct); break;
                case nameof(FraudAnalysis): table = await _reports.GetFraudAnalysisReportAsync(ct); break;
                case nameof(PaymentMethod): table = await _reports.GetPaymentMethodReportAsync(ct); break;
                case nameof(PaymentStatus): table = await _reports.GetPaymentStatusReportAsync(ct); break;
                case nameof(GetRegionalSalesReport): table = await _reports.GetRegionalSalesReportAsync("SUCCESS", ct); break;
                case nameof(SalesByDateRange): table = await _reports.GetSalesReportByDateRangeAsync(start, end, ct); break;
                case nameof(ShipmentCompany): table = await _reports.GetShipmentCompanyReportAsync(ct); break;
                case nameof(PerformanceSystemReport): set = await _reports.GetPerformanceSystemReportAsync(start, end, ct); break;
                case nameof(FinancialReport): set = await _reports.GetFinancialReportAsync(start, end, ct); break;
                case nameof(FraudRiskReport): set = await _reports.GetFraudRiskReportAsync(start, end, ct); break;
                case nameof(OrderVolumeReport): set = await _reports.GetOrderVolumeReportAsync(start, end, ct); break;
                case nameof(PaymentTransactionReport): set = await _reports.GetPaymentTransactionReportAsync(start, end, ct); break;
                case nameof(ProductSummary): set = await _reports.GetProductSummaryReportAsync(cancellationToken: ct); break;
                case nameof(PriceAnalysis): set = await _reports.GetPriceAnalysisReportAsync(cancellationToken: ct); break;
                case nameof(ProductInventory): set = await _reports.GetProductInventoryReportAsync(cancellationToken: ct); break;
                case nameof(ProductStatsByDateRange): set = await _reports.GetProductStatsByDateRangeAsync(start, end, ct); break;
                default: return NotFound();
            }

            var fileBase = $"{actionName}-{DateTime.Now:yyyy-MM-dd}";
            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = table is not null ? _export.ToCsv(table) : _export.ToCsv(set!);
                return File(bytes, "text/csv", fileBase + ".csv");
            }

            var excel = table is not null ? _export.ToExcel(table, actionName) : _export.ToExcel(set!, actionName);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileBase + ".xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report export failed for {Action}", actionName);
            SetTempStatus("Export failed: " + ex.Message, isError: true);
            return RedirectToAction(nameof(Index));
        }
    }

    private IActionResult DateFilterView(string title, string actionName)
    {
        var model = new ReportResultViewModel
        {
            Title = title,
            ActionName = actionName,
            StartDate = DateTime.Today.AddMonths(-1),
            EndDate = DateTime.Today
        };
        return View("DateFilter", model);
    }

    private async Task<IActionResult> TableReportAsync(string title, string actionName, Func<Task<DataTable>> loader, CancellationToken ct)
    {
        var model = await BuildTableModelAsync(title, actionName, loader, ct).ConfigureAwait(false);
        return View("ReportResult", model);
    }

    private async Task<IActionResult> SetReportAsync(string title, string actionName, Func<Task<DataSet>> loader, DateTime start, DateTime end, CancellationToken ct)
    {
        var model = await BuildSetModelAsync(title, actionName, loader, ct).ConfigureAwait(false);
        model.StartDate = start;
        model.EndDate = end;
        return View("ReportResult", model);
    }

    private async Task<ReportResultViewModel> BuildTableModelAsync(string title, string actionName, Func<Task<DataTable>> loader, CancellationToken ct)
    {
        var model = new ReportResultViewModel { Title = title, ActionName = actionName };
        try
        {
            model.Table = await loader().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report {Action} failed", actionName);
            model.Error = ex.Message;
            model.Table = new DataTable();
        }

        return model;
    }

    private async Task<ReportResultViewModel> BuildSetModelAsync(string title, string actionName, Func<Task<DataSet>> loader, CancellationToken ct)
    {
        var model = new ReportResultViewModel { Title = title, ActionName = actionName };
        try
        {
            model.DataSet = await loader().ConfigureAwait(false);
            if (model.DataSet.Tables.Count > 0)
            {
                model.Table = model.DataSet.Tables[0];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report {Action} failed", actionName);
            model.Error = ex.Message;
            model.Table = new DataTable();
        }

        return model;
    }
}
