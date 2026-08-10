using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ReportController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ReportService _reportService { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> CouponUsage(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetCouponUsageReportAsync(cancellationToken);
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in CouponUsage report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> FraudAnalysis(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetFraudAnalysisReportAsync(cancellationToken);
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in FraudAnalysis report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> PaymentMethod(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetPaymentMethodReportAsync(cancellationToken);
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PaymentMethod report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> PaymentStatus(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetPaymentStatusReportAsync(cancellationToken);
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PaymentStatus report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetRegionalSalesReport(CancellationToken cancellationToken, string paymentStatus = "SUCCESS")
        {
            try
            {
                ViewBag.PaymentStatus = paymentStatus ?? string.Empty;
                var report = await _reportService.GetRegionalSalesReportAsync(paymentStatus, cancellationToken);
                // Explicit view name: action is GetRegionalSalesReport, view file is RegionalSales.cshtml
                return View("RegionalSales", report);
            }
            catch (Exception ex)
            {
                // Avoid opaque IIS 500; show empty report with a clear message for admins.
                Logger.Error(ex, "Error in RegionalSales report");
                TempData["StatusMessage"] = "Bölgesel satış raporu yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                ViewBag.PaymentStatus = paymentStatus ?? string.Empty;
                return View("RegionalSales", new DataTable());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("GetRegionalSalesReport")]
        public async Task<ActionResult> GetRegionalSalesReportPost(CancellationToken cancellationToken, string paymentStatus)
        {
            return await GetRegionalSalesReport(cancellationToken, paymentStatus ?? string.Empty);
        }

        [HttpGet]
        public async Task<ActionResult> SalesByDateRange(CancellationToken cancellationToken, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Honor query-string quick links from Index; otherwise default to last month
                var resolvedEnd = endDate ?? DateTime.Today;
                var resolvedStart = startDate ?? resolvedEnd.AddMonths(-1);

                ViewBag.StartDate = resolvedStart.ToString("yyyy-MM-dd");
                ViewBag.EndDate = resolvedEnd.ToString("yyyy-MM-dd");

                // Auto-load when both dates are supplied via query string
                if (startDate.HasValue && endDate.HasValue)
                {
                    if (resolvedStart > resolvedEnd)
                    {
                        ModelState.AddModelError("", "Start date cannot be after end date");
                        return View();
                    }

                    var report = await _reportService.GetSalesReportByDateRangeAsync(resolvedStart, resolvedEnd, cancellationToken);
                    return View(report);
                }

                return View();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading SalesByDateRange view");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SalesByDateRange(CancellationToken cancellationToken, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                    ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
                    return View();
                }

                var report = await _reportService.GetSalesReportByDateRangeAsync(startDate, endDate, cancellationToken);
                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in SalesByDateRange report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ShipmentCompany(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetShipmentCompanyReportAsync(cancellationToken);
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ShipmentCompany report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// GET companion: empty filter form with last-30-days defaults (bookmark / Index links).
        /// </summary>
        [HttpGet]
        public ActionResult PerformanceSystemReport()
        {
            return View("DataSetReportView", CreateEmptyDateRangeModel("PerformanceSystemReport", "Performance System Report"));
        }

        [HttpPost]
        public async Task<ActionResult> PerformanceSystemReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", CreateEmptyDateRangeModel("PerformanceSystemReport", "Performance System Report",
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetPerformanceSystemReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "PerformanceSystemReport",
                    ReportTitle = "Performance System Report",
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View("DataSetReportView", model);
            }

            return View("DataSetReportView", CreateEmptyDateRangeModel("PerformanceSystemReport", "Performance System Report"));
        }

        [HttpGet]
        public ActionResult FinancialReport()
        {
            return View("DataSetReportView", CreateEmptyDateRangeModel("FinancialReport", "Financial Report"));
        }

        [HttpPost]
        public async Task<ActionResult> FinancialReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", CreateEmptyDateRangeModel("FinancialReport", "Financial Report",
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetFinancialReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "FinancialReport",
                    ReportTitle = "Financial Report",
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View("DataSetReportView", model);
            }

            return View("DataSetReportView", CreateEmptyDateRangeModel("FinancialReport", "Financial Report"));
        }

        [HttpGet]
        public ActionResult FraudRiskReport()
        {
            return View("DataSetReportView", CreateEmptyDateRangeModel("FraudRiskReport", "Fraud Risk Report"));
        }

        [HttpPost]
        public async Task<ActionResult> FraudRiskReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", CreateEmptyDateRangeModel("FraudRiskReport", "Fraud Risk Report",
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetFraudRiskReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "FraudRiskReport",
                    ReportTitle = "Fraud Risk Report",
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View("DataSetReportView", model);
            }

            return View("DataSetReportView", CreateEmptyDateRangeModel("FraudRiskReport", "Fraud Risk Report"));
        }

        [HttpGet]
        public ActionResult OrderVolumeReport()
        {
            return View("DataSetReportView", CreateEmptyDateRangeModel("OrderVolumeReport", "Order Volume Report"));
        }

        [HttpPost]
        public async Task<ActionResult> OrderVolumeReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", CreateEmptyDateRangeModel("OrderVolumeReport", "Order Volume Report",
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetOrderVolumeReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "OrderVolumeReport",
                    ReportTitle = "Order Volume Report",
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View("DataSetReportView", model);
            }

            return View("DataSetReportView", CreateEmptyDateRangeModel("OrderVolumeReport", "Order Volume Report"));
        }

        [HttpGet]
        public ActionResult PaymentTransactionReport()
        {
            return View("DataSetReportView", CreateEmptyDateRangeModel("PaymentTransactionReport", "Payment Transaction Report"));
        }

        [HttpPost]
        public async Task<ActionResult> PaymentTransactionReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", CreateEmptyDateRangeModel("PaymentTransactionReport", "Payment Transaction Report",
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetPaymentTransactionReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "PaymentTransactionReport",
                    ReportTitle = "Payment Transaction Report",
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View("DataSetReportView", model);
            }

            return View("DataSetReportView", CreateEmptyDateRangeModel("PaymentTransactionReport", "Payment Transaction Report"));
        }

        [HttpGet]
        public async Task<ActionResult> ProductSummary(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetProductSummaryReportAsync(cancellationToken: cancellationToken);
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "ProductSummary",
                    ReportTitle = "Product Summary"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ProductSummary report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProductSummary(DateTime? startDate, DateTime? endDate, bool? isActive, int? productCategoryId)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View();
                }

                var report = await _reportService.GetProductSummaryReportAsync(startDate, endDate, isActive, productCategoryId);

                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = isActive,
                    ProductCategoryId = productCategoryId,
                    ReportActionName = "ProductSummary",
                    ReportTitle = "Product Summary"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ProductSummary report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> PriceAnalysis(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetPriceAnalysisReportAsync(cancellationToken: cancellationToken);
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "PriceAnalysis",
                    ReportTitle = "Price Analysis"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PriceAnalysis report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PriceAnalysis(decimal? minPrice, decimal? maxPrice, int? productCategoryId)
        {
            try
            {
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    ModelState.AddModelError("", "Minimum price cannot be greater than maximum price");
                    return View(new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        ProductCategoryId = productCategoryId,
                        ReportActionName = "PriceAnalysis",
                        ReportTitle = "Price Analysis"
                    });
                }

                var report = await _reportService.GetPriceAnalysisReportAsync(minPrice, maxPrice, productCategoryId);
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    ProductCategoryId = productCategoryId,
                    ReportActionName = "PriceAnalysis",
                    ReportTitle = "Price Analysis"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PriceAnalysis report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ProductInventory(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetProductInventoryReportAsync(cancellationToken: cancellationToken);
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = "ProductInventory",
                    ReportTitle = "Product Inventory"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ProductInventory report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProductInventory(string state, bool? isCampaign, bool? mainPage)
        {
            try
            {
                var report = await _reportService.GetProductInventoryReportAsync(state, isCampaign, mainPage);
                ViewBag.State = state;
                ViewBag.IsCampaign = isCampaign;
                ViewBag.MainPage = mainPage;
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    State = state,
                    IsCampaign = isCampaign,
                    MainPage = mainPage,
                    ReportActionName = "ProductInventory",
                    ReportTitle = "Product Inventory"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ProductInventory report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ProductStatsByDateRange(CancellationToken cancellationToken)
        {
            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddMonths(-1);

                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
                var report = await _reportService.GetProductStatsByDateRangeAsync(startDate, endDate, cancellationToken);
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    StartDate = startDate,
                    EndDate = endDate,
                    ReportActionName = "ProductStatsByDateRange",
                    ReportTitle = "Product Stats By DateRange"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading ProductStatsByDateRange view");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProductStatsByDateRange(DataSetReportViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.ReportActionName = "ProductStatsByDateRange";
                    model.ReportTitle = "Product Stats By DateRange";
                    return View(model);
                }

                var startDate = model.StartDate.Value;
                var endDate = model.EndDate.Value;

                if (startDate > endDate)
                {
                    ModelState.AddModelError("StartDate", "Start date must be before end date.");
                    model.ReportActionName = "ProductStatsByDateRange";
                    model.ReportTitle = "Product Stats By DateRange";
                    return View(model);
                }

                var report = await _reportService.GetProductStatsByDateRangeAsync(startDate, endDate);
                model.ReportData = report;
                model.ReportActionName = "ProductStatsByDateRange";
                model.ReportTitle = "Product Stats By DateRange";

                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

                return View(model);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error processing ProductStatsByDateRange POST");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Filter-aware export: re-runs the same ReportService method as the page view, then returns Excel or CSV.
        /// Route: /Admin/Report/Export?reportKey=...&amp;format=excel|csv&amp;...filters
        /// </summary>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> Export(
            CancellationToken cancellationToken,
            string reportKey,
            string format,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? productCategoryId = null,
            bool? isActive = null,
            string state = null,
            bool? isCampaign = null,
            bool? mainPage = null,
            string paymentStatus = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportKey))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "reportKey is required.");
                }

                var isCsv = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
                var isExcel = string.IsNullOrWhiteSpace(format)
                    || string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(format, "xls", StringComparison.OrdinalIgnoreCase);

                if (!isCsv && !isExcel)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "format must be excel or csv.");
                }

                // Resolve report data with the same filters the page uses
                object reportData = await LoadReportDataForExportAsync(
                    cancellationToken,
                    reportKey,
                    startDate,
                    endDate,
                    minPrice,
                    maxPrice,
                    productCategoryId,
                    isActive,
                    state,
                    isCampaign,
                    mainPage,
                    paymentStatus);

                if (reportData == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unknown reportKey or missing required filters.");
                }

                var fileBaseName = reportKey;

                if (reportData is DataTable dataTable)
                {
                    if (string.IsNullOrEmpty(dataTable.TableName))
                    {
                        dataTable.TableName = reportKey;
                    }

                    // Single-table Excel can reuse BaseAdminController helper (filename + .xls)
                    if (isExcel)
                    {
                        return DownloadFileDataTable(dataTable, fileBaseName);
                    }

                    return DownloadReportCsv(dataTable, fileBaseName);
                }

                if (reportData is DataSet dataSet)
                {
                    var tables = DataSetToTableList(dataSet, reportKey);
                    if (isExcel)
                    {
                        return DownloadReportExcel(tables, fileBaseName);
                    }

                    // CSV: first sheet, or concatenate sheets with a blank separator row
                    return DownloadReportCsv(tables, fileBaseName);
                }

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Unsupported report data type.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error exporting report {0} as {1}", reportKey, format);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Maps reportKey to the matching ReportService call (same signatures as page actions).
        /// </summary>
        private async Task<object> LoadReportDataForExportAsync(
            CancellationToken cancellationToken,
            string reportKey,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minPrice,
            decimal? maxPrice,
            int? productCategoryId,
            bool? isActive,
            string state,
            bool? isCampaign,
            bool? mainPage,
            string paymentStatus = null)
        {
            switch (reportKey)
            {
                case "CouponUsage":
                    return await _reportService.GetCouponUsageReportAsync(cancellationToken);

                case "FraudAnalysis":
                    return await _reportService.GetFraudAnalysisReportAsync(cancellationToken);

                case "PaymentMethod":
                    return await _reportService.GetPaymentMethodReportAsync(cancellationToken);

                case "PaymentStatus":
                    return await _reportService.GetPaymentStatusReportAsync(cancellationToken);

                case "GetRegionalSalesReport":
                    return await _reportService.GetRegionalSalesReportAsync(paymentStatus, cancellationToken);

                case "SalesByDateRange":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetSalesReportByDateRangeAsync(startDate.Value, endDate.Value, cancellationToken);

                case "ShipmentCompany":
                    return await _reportService.GetShipmentCompanyReportAsync(cancellationToken);

                case "PerformanceSystemReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetPerformanceSystemReportAsync(startDate.Value, endDate.Value, cancellationToken);

                case "FinancialReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetFinancialReportAsync(startDate.Value, endDate.Value, cancellationToken);

                case "FraudRiskReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetFraudRiskReportAsync(startDate.Value, endDate.Value, cancellationToken);

                case "OrderVolumeReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetOrderVolumeReportAsync(startDate.Value, endDate.Value, cancellationToken);

                case "PaymentTransactionReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetPaymentTransactionReportAsync(startDate.Value, endDate.Value, cancellationToken);

                case "ProductSummary":
                    return await _reportService.GetProductSummaryReportAsync(startDate, endDate, isActive, productCategoryId, cancellationToken);

                case "PriceAnalysis":
                    return await _reportService.GetPriceAnalysisReportAsync(minPrice, maxPrice, productCategoryId, cancellationToken);

                case "ProductInventory":
                    return await _reportService.GetProductInventoryReportAsync(state, isCampaign, mainPage, cancellationToken);

                case "ProductStatsByDateRange":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return await _reportService.GetProductStatsByDateRangeAsync(startDate.Value, endDate.Value, cancellationToken);

                default:
                    return null;
            }
        }

        private object LoadReportDataForExport(
            string reportKey,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minPrice,
            decimal? maxPrice,
            int? productCategoryId,
            bool? isActive,
            string state,
            bool? isCampaign,
            bool? mainPage,
            string paymentStatus = null)
        {
            switch (reportKey)
            {
                case "CouponUsage":
                    return _reportService.GetCouponUsageReport();

                case "FraudAnalysis":
                    return _reportService.GetFraudAnalysisReport();

                case "PaymentMethod":
                    return _reportService.GetPaymentMethodReport();

                case "PaymentStatus":
                    return _reportService.GetPaymentStatusReport();

                case "GetRegionalSalesReport":
                    return _reportService.GetRegionalSalesReport(paymentStatus);

                case "SalesByDateRange":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetSalesReportByDateRange(startDate.Value, endDate.Value);

                case "ShipmentCompany":
                    return _reportService.GetShipmentCompanyReport();

                case "PerformanceSystemReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetPerformanceSystemReport(startDate.Value, endDate.Value);

                case "FinancialReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetFinancialReport(startDate.Value, endDate.Value);

                case "FraudRiskReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetFraudRiskReport(startDate.Value, endDate.Value);

                case "OrderVolumeReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetOrderVolumeReport(startDate.Value, endDate.Value);

                case "PaymentTransactionReport":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetPaymentTransactionReport(startDate.Value, endDate.Value);

                case "ProductSummary":
                    return _reportService.GetProductSummaryReport(startDate, endDate, isActive, productCategoryId);

                case "PriceAnalysis":
                    return _reportService.GetPriceAnalysisReport(minPrice, maxPrice, productCategoryId);

                case "ProductInventory":
                    return _reportService.GetProductInventoryReport(state, isCampaign, mainPage);

                case "ProductStatsByDateRange":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        return null;
                    }
                    return _reportService.GetProductStatsByDateRange(startDate.Value, endDate.Value);

                default:
                    return null;
            }
        }

        private static DataSetReportViewModel CreateEmptyDateRangeModel(
            string actionName,
            string title,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            // Default empty form: last 30 days
            var resolvedEnd = endDate ?? DateTime.Today;
            var resolvedStart = startDate ?? resolvedEnd.AddDays(-30);

            return new DataSetReportViewModel
            {
                ReportData = new DataSet(),
                ReportActionName = actionName,
                ReportTitle = title,
                StartDate = resolvedStart,
                EndDate = resolvedEnd
            };
        }

        private static List<DataTable> DataSetToTableList(DataSet dataSet, string fallbackName)
        {
            var tables = new List<DataTable>();
            if (dataSet == null || dataSet.Tables.Count == 0)
            {
                var empty = new DataTable(fallbackName);
                tables.Add(empty);
                return tables;
            }

            for (int i = 0; i < dataSet.Tables.Count; i++)
            {
                var table = dataSet.Tables[i];
                if (string.IsNullOrEmpty(table.TableName))
                {
                    table.TableName = string.Format("{0}_{1}", fallbackName, i + 1);
                }
                tables.Add(table);
            }

            return tables;
        }

        /// <summary>
        /// Multi-sheet Excel download via ExcelHelper List&lt;DataTable&gt; overload. Filename: {name}-{yyyy-MM-dd}.xls
        /// </summary>
        private ActionResult DownloadReportExcel(List<DataTable> tables, string fileName)
        {
            var stampedName = string.Format("{1}-{0}", DateTime.Now.ToString("yyyy-MM-dd"), fileName);
            var bytes = ExcelHelper.GetExcelByteArrayFromDataTable(tables);
            return File(bytes, "application/vnd.ms-excel", stampedName + ".xls");
        }

        /// <summary>
        /// Single-table CSV for Turkish Excel (semicolon delimiter, UTF-8 BOM). Filename: {name}-{yyyy-MM-dd}.csv
        /// </summary>
        private ActionResult DownloadReportCsv(DataTable table, string fileName)
        {
            var stampedName = string.Format("{1}-{0}", DateTime.Now.ToString("yyyy-MM-dd"), fileName);
            var data = ExcelHelper.ExportReportCsv(table ?? new DataTable());
            return File(data, "text/csv", stampedName + ".csv");
        }

        /// <summary>
        /// Multi-table CSV: sheets concatenated with a blank separator line between tables.
        /// </summary>
        private ActionResult DownloadReportCsv(List<DataTable> tables, string fileName)
        {
            if (tables == null || tables.Count == 0)
            {
                return DownloadReportCsv(new DataTable(), fileName);
            }

            if (tables.Count == 1)
            {
                return DownloadReportCsv(tables[0], fileName);
            }

            var stampedName = string.Format("{1}-{0}", DateTime.Now.ToString("yyyy-MM-dd"), fileName);
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var sb = new StringBuilder();
            // One BOM + one sep=; for the whole file
            sb.AppendLine("sep=;");

            for (int i = 0; i < tables.Count; i++)
            {
                var tableBytes = ExcelHelper.ExportReportCsv(tables[i] ?? new DataTable());
                // Strip UTF-8 BOM and leading "sep=;\r\n" from each chunk, then append
                string chunk = Encoding.UTF8.GetString(tableBytes);
                if (chunk.Length > 0 && chunk[0] == '\uFEFF')
                {
                    chunk = chunk.Substring(1);
                }
                if (chunk.StartsWith("sep=;\r\n", StringComparison.Ordinal))
                {
                    chunk = chunk.Substring("sep=;\r\n".Length);
                }
                else if (chunk.StartsWith("sep=;\n", StringComparison.Ordinal))
                {
                    chunk = chunk.Substring("sep=;\n".Length);
                }

                if (i > 0)
                {
                    sb.AppendLine();
                }
                sb.Append(chunk.TrimEnd('\r', '\n'));
                sb.AppendLine();
            }

            var body = utf8.GetBytes(sb.ToString());
            var combined = Encoding.UTF8.GetPreamble().Concat(body).ToArray();
            return File(combined, "text/csv", stampedName + ".csv");
        }
    }
}
