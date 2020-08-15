using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        // GET: Info
        public ActionResult Index(string id)
        {
            var page = MenuService.GetPageByMenuLink("info-"+ id,CurrentLanguage);
            if (page == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(page);
        }
    }
}