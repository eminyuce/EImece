using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories;
using Ninject;
using System.Threading.Tasks;

namespace EImece.Areas.Admin.Controllers
{
    [AllowAnonymous]
    public class TestController : BaseAdminController
    {
        // GET: Admin/Test
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TestGridView()
        {
            return View();
        }
        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult Index3()
        {
            return View();
        }

        [Inject]
        public MigrationRepository MigrationRepository { get;set;}

        public ActionResult MigrationData()
        {
            String siteUrl = "http://atlantiscam.com";
            MigrationRepository.SiteUrl = siteUrl;
            MigrationRepository.MigrateImages(CurrentLanguage);

            return Content("Done");
        }
        public ActionResult getData(int id=0)
        {
            //Datatable parameter
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //paging parameter
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //sorting parameter
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //filter parameter
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            List<Product> products = new List<Product>();
            int pageSize = length.ToInt();
            int skip = start.ToInt();
            int recordsTotal = 0;
            //Database query
            var v = ProductService.GetAll();
                //search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    v = v.Where(a =>
                        a.ProductCode.Contains(searchValue) ||
                        a.Name.Contains(searchValue) 
                        ).ToList();
                }

            //sort
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                //for make sort simpler we will add Syste.Linq.Dynamic reference
                v = v.OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            else
            {
                v = v.OrderBy(r => r.Id).ToList();
            }

            recordsTotal = v.Count();
                products = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = products });
        }

         
    }
}