using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EImece.Domain.Models.AdminModels;

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
        public ActionResult RegionalSales()
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

    }
}