using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{

    public class DashboardController : BaseAdminController
    {
        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SearchContent(String searchContent)
        {
            String search = searchContent;
            var resultList = new List<BaseContent>();
            ViewBag.SearchKey = search;
            if (String.IsNullOrEmpty(search))
            {
                var urlReferrer = Request.UrlReferrer;
                if (urlReferrer != null)
                {
                    return Redirect(urlReferrer.ToStr());
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(ProductCategoryService.SearchEntities(whereLambda1, search, CurrentLanguage));

            Expression<Func<Product, bool>> whereLambda2 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(ProductService.SearchEntities(whereLambda2, search, CurrentLanguage));

            Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(StoryCategoryService.SearchEntities(whereLambda3, search, CurrentLanguage));

            Expression<Func<Story, bool>> whereLambda4 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(StoryService.SearchEntities(whereLambda4, search, CurrentLanguage));

            Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            resultList.AddRange(MenuService.SearchEntities(whereLamba5, search, CurrentLanguage));

            return View(resultList);
        }
        public ActionResult ClearCache()
        {
            MemoryCacheProvider.ClearAll();

        
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                return Redirect(urlReferrer.ToStr());
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MemoryCacheProvider.ClearAll();
            return RedirectToAction("Index", "Home",new { @area="" });
        }
        public ActionResult SetLanguage(string name)
        {
            //  name = CultureHelper.GetImplementedCulture(name);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(name);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            Response.Cookies[AdminCultureCookieName].Value = name;
            MemoryCacheProvider.ClearAll();
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                return Redirect(urlReferrer.ToStr());
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Contact3()
        {
            String path = "~/App_Data/";
            var pathName = Server.MapPath(path);
            var files = Directory.GetFiles(pathName).ToList();
            ViewBag.PathName = pathName;
            return View(files);
        }
        public ActionResult Contact2(HttpPostedFileBase excelFile = null)
        {
            ViewBag.Message = "Your contact page.";

            String path = "~/App_Data/";
            var r = new Random();
            string fileName = r.Next(0, 1000) + excelFile.FileName;
            var pathName = Server.MapPath(path + fileName);
            excelFile.SaveAs(pathName);
            //DataSet ds = ExcelHelper.GetDS(pathName, ExcelHelper.GetWorkSheets(pathName).FirstOrDefault());
            //return View(ds.Tables[0]);
            return RedirectToAction("DisplayTable", new { id = fileName });

        }

        public ActionResult DisplayTable(String id)
        {
            String path = "~/App_Data/";
            var pathName = Server.MapPath(path +"/"+ id);
            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName, 0);
            return View(dt);
        }

        public ActionResult DisplayExcel(String id)
        {
            String fileName = id+ ".xlsx";
            String path = "~/App_Data/";
            var pathName = Server.MapPath(path + fileName);

            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName,0);
            List<ProductCategory> companyList = dt.ConvertToList<ProductCategory>();
            ViewBag.DataTable = dt;
            return View(companyList);
        }

    }
}