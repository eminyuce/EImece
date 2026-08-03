using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Data;
using System.Net;
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
        public ActionResult CouponUsage()
        {
            try
            {
                var report = _reportService.GetCouponUsageReport();
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in CouponUsage report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult FraudAnalysis()
        {
            try
            {
                var report = _reportService.GetFraudAnalysisReport();
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in FraudAnalysis report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult PaymentMethod()
        {
            try
            {
                var report = _reportService.GetPaymentMethodReport();
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PaymentMethod report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult PaymentStatus()
        {
            try
            {
                var report = _reportService.GetPaymentStatusReport();
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PaymentStatus report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult GetRegionalSalesReport()
        {
            try
            {
                var report = _reportService.GetRegionalSalesReport();
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in RegionalSales report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult SalesByDateRange()
        {
            try
            {
                // Default to last month
                var endDate = DateTime.Today;
                var startDate = endDate.AddMonths(-1);

                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

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
        public ActionResult SalesByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View();
                }

                var report = _reportService.GetSalesReportByDateRange(startDate, endDate);
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
        public ActionResult ShipmentCompany()
        {
            try
            {
                var report = _reportService.GetShipmentCompanyReport();
                return View(report);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ShipmentCompany report");
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public ActionResult PerformanceSystemReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        ReportActionName = "PerformanceSystemReport",
                        ReportTitle = "Performance System Report",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today
                    });
                }

                DataSet report = _reportService.GetPerformanceSystemReport(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
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
            else
            {
                return View("DataSetReportView", new DataSetReportViewModel
                {
                    ReportData = new DataSet(),
                    ReportActionName = "PerformanceSystemReport",
                    ReportTitle = "Performance System Report",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                });
            }
        }

        [HttpPost]
        public ActionResult FinancialReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        ReportActionName = "FinancialReport",
                        ReportTitle = "Financial Report",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today
                    });
                }

                DataSet report = _reportService.GetFinancialReport(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
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
            else
            {
                return View("DataSetReportView", new DataSetReportViewModel
                {
                    ReportData = new DataSet(),
                    ReportActionName = "FinancialReport",
                    ReportTitle = "Financial Report",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                });
            }
        }

        [HttpPost]
        public ActionResult FraudRiskReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        ReportActionName = "FraudRiskReport",
                        ReportTitle = "Fraud Risk Report",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today
                    });
                }

                DataSet report = _reportService.GetFraudRiskReport(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
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
            else
            {
                return View("DataSetReportView", new DataSetReportViewModel
                {
                    ReportData = new DataSet(),
                    ReportActionName = "FraudRiskReport",
                    ReportTitle = "Fraud Risk Report",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                });
            }
        }

        [HttpPost]
        public ActionResult OrderVolumeReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        ReportActionName = "OrderVolumeReport",
                        ReportTitle = "Order Volume Report",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today
                    });
                }

                DataSet report = _reportService.GetOrderVolumeReport(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
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
            else
            {
                return View("DataSetReportView", new DataSetReportViewModel
                {
                    ReportData = new DataSet(),
                    ReportActionName = "OrderVolumeReport",
                    ReportTitle = "Order Volume Report",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                });
            }
        }

        [HttpPost]
        public ActionResult PaymentTransactionReport(DataSetReportViewModel dataSetReportViewModel)
        {
            if (dataSetReportViewModel != null && dataSetReportViewModel.IsNotEmpty())
            {
                if (dataSetReportViewModel.StartDate > dataSetReportViewModel.EndDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View("DataSetReportView", new DataSetReportViewModel
                    {
                        ReportData = new DataSet(),
                        ReportActionName = "PaymentTransactionReport",
                        ReportTitle = "Payment Transaction Report",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today
                    });
                }

                DataSet report = _reportService.GetPaymentTransactionReport(dataSetReportViewModel.StartDate.Value, dataSetReportViewModel.EndDate.Value);
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
            else
            {
                return View("DataSetReportView", new DataSetReportViewModel
                {
                    ReportData = new DataSet(),
                    ReportActionName = "PaymentTransactionReport",
                    ReportTitle = "Payment Transaction Report",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                });
            }
        }

        [HttpGet]
        public ActionResult ProductSummary()
        {
            try
            {
                var report = _reportService.GetProductSummaryReport();
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
        public ActionResult ProductSummary(DateTime? startDate, DateTime? endDate, bool? isActive, int? productCategoryId)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    ModelState.AddModelError("", "Start date cannot be after end date");
                    return View();
                }

                var report = _reportService.GetProductSummaryReport(startDate, endDate, isActive, productCategoryId);

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
        public ActionResult PriceAnalysis()
        {
            try
            {
                var report = _reportService.GetPriceAnalysisReport();
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
        public ActionResult PriceAnalysis(decimal? minPrice, decimal? maxPrice, int? productCategoryId)
        {
            try
            {
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    ModelState.AddModelError("", "Minimum price cannot be greater than maximum price");
                    return View(new DataSetReportViewModel
                    {
                        ReportData = new DataSet(), // Empty dataset for invalid input
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        ProductCategoryId = productCategoryId,
                        ReportActionName = "PriceAnalysis",
                        ReportTitle = "Price Analysis"
                    });
                }

                var report = _reportService.GetPriceAnalysisReport(minPrice, maxPrice, productCategoryId);
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
        public ActionResult ProductInventory()
        {
            try
            {
                var report = _reportService.GetProductInventoryReport();
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
        public ActionResult ProductInventory(string state, bool? isCampaign, bool? mainPage)
        {
            try
            {
                var report = _reportService.GetProductInventoryReport(state, isCampaign, mainPage);
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
        public ActionResult ProductStatsByDateRange()
        {
            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddMonths(-1);

                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
                var report = _reportService.GetProductStatsByDateRange(startDate, endDate);
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
        public ActionResult ProductStatsByDateRange(DataSetReportViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return the view with validation errors if dates are invalid
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

                var report = _reportService.GetProductStatsByDateRange(startDate, endDate);
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
    }
}