using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
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
using Resources;

namespace EImece.Areas.Admin.Controllers
{
    public class ReportController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string PerformanceSystemReportKey = "PerformanceSystemReport";
        private const string FinancialReportKey = "FinancialReport";
        private const string FraudRiskReportKey = "FraudRiskReport";
        private const string OrderVolumeReportKey = "OrderVolumeReport";
        private const string PaymentTransactionReportKey = "PaymentTransactionReport";
        private const string ProductSummaryKey = "ProductSummary";
        private const string PriceAnalysisKey = "PriceAnalysis";
        private const string ProductInventoryKey = "ProductInventory";
        private const string ProductStatsByDateRangeKey = "ProductStatsByDateRange";
        private const string DataSetReportViewName = "DataSetReportView";
        private const string PerformanceSystemReportTitle = "Performance System Report";
        private const string FinancialReportTitle = "Financial Report";
        private const string FraudRiskReportTitle = "Fraud Risk Report";
        private const string OrderVolumeReportTitle = "Order Volume Report";
        private const string PaymentTransactionReportTitle = "Payment Transaction Report";
        private const string IsoDateFormat = "yyyy-MM-dd";
        private const string StartDateAfterEndDateMessage = "Start date cannot be after end date";
        private readonly ReportService _reportService;

        public ReportController(
            ISettingService settingService,
            ReportService reportService)
            : base(settingService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

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

                ViewBag.StartDate = resolvedStart.ToString(IsoDateFormat);
                ViewBag.EndDate = resolvedEnd.ToString(IsoDateFormat);

                // Auto-load when both dates are supplied via query string
                if (startDate.HasValue && endDate.HasValue)
                {
                    if (resolvedStart > resolvedEnd)
                    {
                        ModelState.AddModelError("", StartDateAfterEndDateMessage);
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
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
                    ViewBag.StartDate = startDate.ToString(IsoDateFormat);
                    ViewBag.EndDate = endDate.ToString(IsoDateFormat);
                    return View();
                }

                var report = await _reportService.GetSalesReportByDateRangeAsync(startDate, endDate, cancellationToken);
                ViewBag.StartDate = startDate.ToString(IsoDateFormat);
                ViewBag.EndDate = endDate.ToString(IsoDateFormat);
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
            return View(DataSetReportViewName, CreateEmptyDateRangeModel(PerformanceSystemReportKey, PerformanceSystemReportTitle));
        }

        [HttpPost]
        public async Task<ActionResult> PerformanceSystemReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
                    return View(DataSetReportViewName, CreateEmptyDateRangeModel(PerformanceSystemReportKey, PerformanceSystemReportTitle,
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetPerformanceSystemReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = PerformanceSystemReportKey,
                    ReportTitle = PerformanceSystemReportTitle,
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View(DataSetReportViewName, model);
            }

            return View(DataSetReportViewName, CreateEmptyDateRangeModel(PerformanceSystemReportKey, PerformanceSystemReportTitle));
        }

        [HttpGet]
        public ActionResult FinancialReport()
        {
            return View(DataSetReportViewName, CreateEmptyDateRangeModel(FinancialReportKey, FinancialReportTitle));
        }

        [HttpPost]
        public async Task<ActionResult> FinancialReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
                    return View(DataSetReportViewName, CreateEmptyDateRangeModel(FinancialReportKey, FinancialReportTitle,
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetFinancialReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = FinancialReportKey,
                    ReportTitle = FinancialReportTitle,
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View(DataSetReportViewName, model);
            }

            return View(DataSetReportViewName, CreateEmptyDateRangeModel(FinancialReportKey, FinancialReportTitle));
        }

        [HttpGet]
        public ActionResult FraudRiskReport()
        {
            return View(DataSetReportViewName, CreateEmptyDateRangeModel(FraudRiskReportKey, FraudRiskReportTitle));
        }

        [HttpPost]
        public async Task<ActionResult> FraudRiskReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
                    return View(DataSetReportViewName, CreateEmptyDateRangeModel(FraudRiskReportKey, FraudRiskReportTitle,
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetFraudRiskReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = FraudRiskReportKey,
                    ReportTitle = FraudRiskReportTitle,
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View(DataSetReportViewName, model);
            }

            return View(DataSetReportViewName, CreateEmptyDateRangeModel(FraudRiskReportKey, FraudRiskReportTitle));
        }

        [HttpGet]
        public ActionResult OrderVolumeReport()
        {
            return View(DataSetReportViewName, CreateEmptyDateRangeModel(OrderVolumeReportKey, OrderVolumeReportTitle));
        }

        [HttpPost]
        public async Task<ActionResult> OrderVolumeReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
                    return View(DataSetReportViewName, CreateEmptyDateRangeModel(OrderVolumeReportKey, OrderVolumeReportTitle,
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetOrderVolumeReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = OrderVolumeReportKey,
                    ReportTitle = OrderVolumeReportTitle,
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View(DataSetReportViewName, model);
            }

            return View(DataSetReportViewName, CreateEmptyDateRangeModel(OrderVolumeReportKey, OrderVolumeReportTitle));
        }

        [HttpGet]
        public ActionResult PaymentTransactionReport()
        {
            return View(DataSetReportViewName, CreateEmptyDateRangeModel(PaymentTransactionReportKey, PaymentTransactionReportTitle));
        }

        [HttpPost]
        public async Task<ActionResult> PaymentTransactionReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
                    return View(DataSetReportViewName, CreateEmptyDateRangeModel(PaymentTransactionReportKey, PaymentTransactionReportTitle,
                        dataSetReportViewModel.StartDate, dataSetReportViewModel.EndDate));
                }

                DataSet report = await _reportService.GetPaymentTransactionReportAsync(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
                var model = new DataSetReportViewModel
                {
                    ReportData = report,
                    ReportActionName = PaymentTransactionReportKey,
                    ReportTitle = PaymentTransactionReportTitle,
                    StartDate = dataSetReportViewModel.StartDate,
                    EndDate = dataSetReportViewModel.EndDate
                };
                return View(DataSetReportViewName, model);
            }

            return View(DataSetReportViewName, CreateEmptyDateRangeModel(PaymentTransactionReportKey, PaymentTransactionReportTitle));
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
                    ReportActionName = ProductSummaryKey,
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
                    ModelState.AddModelError("", StartDateAfterEndDateMessage);
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
                    ReportActionName = ProductSummaryKey,
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
                    ReportActionName = PriceAnalysisKey,
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
                    ModelState.AddModelError("", AdminResource.MinPriceCannotBeGreaterThanMaxPrice);
                    return View(new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        ProductCategoryId = productCategoryId,
                        ReportActionName = PriceAnalysisKey,
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
                    ReportActionName = PriceAnalysisKey,
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
                    ReportActionName = ProductInventoryKey,
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
                    ReportActionName = ProductInventoryKey,
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

                ViewBag.StartDate = startDate.ToString(IsoDateFormat);
                ViewBag.EndDate = endDate.ToString(IsoDateFormat);
                var report = await _reportService.GetProductStatsByDateRangeAsync(startDate, endDate, cancellationToken);
                return View(new DataSetReportViewModel
                {
                    ReportData = report,
                    StartDate = startDate,
                    EndDate = endDate,
                    ReportActionName = ProductStatsByDateRangeKey,
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
                    model.ReportActionName = ProductStatsByDateRangeKey;
                    model.ReportTitle = "Product Stats By DateRange";
                    return View(model);
                }

                var startDate = model.StartDate.Value;
                var endDate = model.EndDate.Value;

                if (startDate > endDate)
                {
                    ModelState.AddModelError("StartDate", AdminResource.StartDateMustBeBeforeEndDate);
                    model.ReportActionName = ProductStatsByDateRangeKey;
                    model.ReportTitle = "Product Stats By DateRange";
                    return View(model);
                }

                var report = await _reportService.GetProductStatsByDateRangeAsync(startDate, endDate);
                model.ReportData = report;
                model.ReportActionName = ProductStatsByDateRangeKey;
                model.ReportTitle = "Product Stats By DateRange";

                ViewBag.StartDate = startDate.ToString(IsoDateFormat);
                ViewBag.EndDate = endDate.ToString(IsoDateFormat);

                return View(model);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error processing ProductStatsByDateRange POST");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        #region User Audit Reports

        [HttpGet]
        public async Task<ActionResult> UserAudit(CancellationToken cancellationToken, DateTime? startDate = null, DateTime? endDate = null, string userId = null, string tableName = null, string actionType = null, string tab = null)
        {
            try
            {
                var model = new UserAuditReportViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId,
                    TableName = tableName,
                    ActionType = actionType,
                    ActiveTab = string.IsNullOrWhiteSpace(tab) ? "summary" : tab
                };

                await PopulateAuditReportDataAsync(model, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in UserAudit GET report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAudit(UserAuditReportViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                if (model == null)
                {
                    model = new UserAuditReportViewModel();
                }

                if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
                {
                    ModelState.AddModelError("StartDate", AdminResource.StartDateMustBeBeforeEndDate);
                }

                await PopulateAuditReportDataAsync(model, cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in UserAudit POST report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private async Task PopulateAuditReportDataAsync(UserAuditReportViewModel model, CancellationToken cancellationToken)
        {
            var usersTable = await _reportService.GetAuditUsersListAsync(cancellationToken);
            model.AvailableUsers = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = AdminResource.AllUsers }
            };
            if (usersTable != null)
            {
                foreach (DataRow row in usersTable.Rows)
                {
                    var uId = row["UserId"]?.ToString() ?? "";
                    var uFullName = row["FullName"]?.ToString() ?? "";
                    var uName = row["UserName"]?.ToString() ?? "";
                    var text = !string.IsNullOrWhiteSpace(uFullName) && uFullName != "Unknown"
                        ? $"{uFullName} ({uName})"
                        : uName;

                    model.AvailableUsers.Add(new SelectListItem
                    {
                        Value = uId,
                        Text = text,
                        Selected = string.Equals(model.UserId, uId, StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            var tablesTable = await _reportService.GetAuditTablesListAsync(cancellationToken);
            model.AvailableTables = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = AdminResource.AllTables }
            };
            if (tablesTable != null)
            {
                foreach (DataRow row in tablesTable.Rows)
                {
                    var tbl = row["TableName"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(tbl))
                    {
                        model.AvailableTables.Add(new SelectListItem
                        {
                            Value = tbl,
                            Text = tbl,
                            Selected = string.Equals(model.TableName, tbl, StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
            }

            model.AvailableActionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = AdminResource.AllActions, Selected = string.IsNullOrEmpty(model.ActionType) || model.ActionType == "All" },
                new SelectListItem { Value = "Created", Text = AdminResource.CreatedOnly, Selected = model.ActionType == "Created" },
                new SelectListItem { Value = "Updated", Text = AdminResource.UpdatedOnly, Selected = model.ActionType == "Updated" }
            };

            model.UserSummaryData = await _reportService.GetUserAuditSummaryReportAsync(model.StartDate, model.EndDate, model.UserId, model.TableName, cancellationToken) ?? new DataTable();
            model.UserSummaryData.TableName = "UserAuditSummary";

            model.MonthlyBreakdownData = await _reportService.GetUserAuditMonthlyBreakdownAsync(model.StartDate, model.EndDate, model.UserId, model.TableName, cancellationToken) ?? new DataTable();
            model.MonthlyBreakdownData.TableName = "UserAuditMonthlyBreakdown";

            model.DetailedRecordsData = await _reportService.GetUserAuditDetailedRecordsAsync(model.StartDate, model.EndDate, model.UserId, model.TableName, model.ActionType, cancellationToken) ?? new DataTable();
            model.DetailedRecordsData.TableName = "UserAuditDetailed";

            if (model.UserSummaryData != null && model.UserSummaryData.Rows.Count > 0)
            {
                model.TotalUsersCount = model.UserSummaryData.Rows.Count;
                int totalCreated = 0;
                int totalUpdated = 0;
                int totalActivity = 0;
                foreach (DataRow row in model.UserSummaryData.Rows)
                {
                    totalCreated += row["CreatedCount"] != DBNull.Value ? System.Convert.ToInt32(row["CreatedCount"]) : 0;
                    totalUpdated += row["UpdatedCount"] != DBNull.Value ? System.Convert.ToInt32(row["UpdatedCount"]) : 0;
                    totalActivity += row["TotalActivity"] != DBNull.Value ? System.Convert.ToInt32(row["TotalActivity"]) : 0;
                }
                model.TotalCreatedCount = totalCreated;
                model.TotalUpdatedCount = totalUpdated;
                model.TotalActivityCount = totalActivity;
            }
        }

        #endregion User Audit Reports

        /// <summary>
        /// Filter-aware export: re-runs the same ReportService method as the page view, then returns Excel or CSV.
        /// Route: /Admin/Report/Export?reportKey=...&amp;format=excel|csv&amp;...filters
        /// </summary>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> Export(CancellationToken cancellationToken, ReportExportFilter filter)
        {
            string reportKey = filter != null ? filter.ReportKey : null;
            string format = filter != null ? filter.Format : null;
            try
            {
                if (filter == null || string.IsNullOrWhiteSpace(filter.ReportKey))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "reportKey is required.");
                }

                reportKey = filter.ReportKey;
                format = filter.Format;

                var rawStart = Request?["startDate"] ?? Request?["StartDate"];
                var rawEnd = Request?["endDate"] ?? Request?["EndDate"];

                if (!string.IsNullOrWhiteSpace(rawStart))
                {
                    filter.StartDate = GeneralHelper.TryParseFlexibleDate(rawStart) ?? filter.StartDate;
                }
                if (!string.IsNullOrWhiteSpace(rawEnd))
                {
                    filter.EndDate = GeneralHelper.TryParseFlexibleDate(rawEnd) ?? filter.EndDate;
                }
                var isCsv = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
                var isExcel = string.IsNullOrWhiteSpace(format)
                    || string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(format, "xls", StringComparison.OrdinalIgnoreCase);

                if (!isCsv && !isExcel)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "format must be excel or csv.");
                }

                object reportData = await LoadReportDataForExportAsync(cancellationToken, filter);

                if (reportData == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unknown reportKey or missing required filters.");
                }

                return CreateExportFileResult(reportData, reportKey, isExcel);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error exporting report {0} as {1}", reportKey, format);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private ActionResult CreateExportFileResult(object reportData, string reportKey, bool isExcel)
        {
            var fileBaseName = reportKey;

            if (reportData is DataTable dataTable)
            {
                if (string.IsNullOrEmpty(dataTable.TableName))
                {
                    dataTable.TableName = reportKey;
                }

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

                return DownloadReportCsv(tables, fileBaseName);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Unsupported report data type.");
        }

        /// <summary>
        /// Maps reportKey to the matching ReportService call (same signatures as page actions).
        /// </summary>
        private async Task<object> LoadReportDataForExportAsync(CancellationToken cancellationToken, ReportExportFilter filter)
        {
            var reportKey = filter.ReportKey;
            var startDate = filter.StartDate;
            var endDate = filter.EndDate;
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
                    return await _reportService.GetRegionalSalesReportAsync(filter.PaymentStatus, cancellationToken);

                case "SalesByDateRange":
                    return await LoadDateRangeReportAsync(reportKey, startDate, endDate, cancellationToken);

                case "ShipmentCompany":
                    return await _reportService.GetShipmentCompanyReportAsync(cancellationToken);

                case PerformanceSystemReportKey:
                case FinancialReportKey:
                case FraudRiskReportKey:
                case OrderVolumeReportKey:
                case PaymentTransactionReportKey:
                case ProductStatsByDateRangeKey:
                    return await LoadDateRangeReportAsync(reportKey, startDate, endDate, cancellationToken);

                case ProductSummaryKey:
                    return await _reportService.GetProductSummaryReportAsync(startDate, endDate, filter.IsActive, filter.ProductCategoryId, cancellationToken);

                case PriceAnalysisKey:
                    return await _reportService.GetPriceAnalysisReportAsync(filter.MinPrice, filter.MaxPrice, filter.ProductCategoryId, cancellationToken);

                case ProductInventoryKey:
                    return await _reportService.GetProductInventoryReportAsync(filter.State, filter.IsCampaign, filter.MainPage, cancellationToken);

                case "UserAudit":
                case "UserAuditSummary":
                    return await _reportService.GetUserAuditSummaryReportAsync(startDate, endDate, filter.UserId, filter.TableName, cancellationToken);

                case "UserAuditMonthlyBreakdown":
                    return await _reportService.GetUserAuditMonthlyBreakdownAsync(startDate, endDate, filter.UserId, filter.TableName, cancellationToken);

                case "UserAuditDetailed":
                    return await _reportService.GetUserAuditDetailedRecordsAsync(startDate, endDate, filter.UserId, filter.TableName, filter.ActionType, cancellationToken);

                default:
                    return null;
            }
        }

        private async Task<object> LoadDateRangeReportAsync(string reportKey, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return null;
            }

            var start = startDate.Value;
            var end = endDate.Value;
            if (reportKey == "SalesByDateRange")
            {
                return await _reportService.GetSalesReportByDateRangeAsync(start, end, cancellationToken);
            }
            if (reportKey == PerformanceSystemReportKey)
            {
                return await _reportService.GetPerformanceSystemReportAsync(start, end, cancellationToken);
            }
            if (reportKey == FinancialReportKey)
            {
                return await _reportService.GetFinancialReportAsync(start, end, cancellationToken);
            }
            if (reportKey == FraudRiskReportKey)
            {
                return await _reportService.GetFraudRiskReportAsync(start, end, cancellationToken);
            }
            if (reportKey == OrderVolumeReportKey)
            {
                return await _reportService.GetOrderVolumeReportAsync(start, end, cancellationToken);
            }
            if (reportKey == PaymentTransactionReportKey)
            {
                return await _reportService.GetPaymentTransactionReportAsync(start, end, cancellationToken);
            }
            if (reportKey == ProductStatsByDateRangeKey)
            {
                return await _reportService.GetProductStatsByDateRangeAsync(start, end, cancellationToken);
            }

            return null;
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
            var stampedName = string.Format("{1}-{0}", DateTime.Now.ToString(IsoDateFormat), fileName);
            var bytes = ExcelHelper.GetExcelByteArrayFromDataTable(tables);
            return File(bytes, "application/vnd.ms-excel", stampedName + ".xls");
        }

        /// <summary>
        /// Single-table CSV for Turkish Excel (semicolon delimiter, UTF-8 BOM). Filename: {name}-{yyyy-MM-dd}.csv
        /// </summary>
        private ActionResult DownloadReportCsv(DataTable table, string fileName)
        {
            var stampedName = string.Format("{1}-{0}", DateTime.Now.ToString(IsoDateFormat), fileName);
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

            var stampedName = string.Format("{1}-{0}", DateTime.Now.ToString(IsoDateFormat), fileName);
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

    public class ReportExportFilter
    {
        public string ReportKey { get; set; }
        public string Format { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? ProductCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string State { get; set; }
        public bool? IsCampaign { get; set; }
        public bool? MainPage { get; set; }
        public string PaymentStatus { get; set; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public string ActionType { get; set; }
    }
}
